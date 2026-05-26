import React, { useState, useEffect, useRef, useCallback ,useMemo } from "react";
import axios from "axios";
import debounce from "lodash.debounce";
import config from '../../src/config';

const pre_view_count = 10;

function generateUUID() {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }

  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.random() * 16 | 0;
    // eslint-disable-next-line no-mixed-operators
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export const Advancedselect_trigger = (defaultField = "SERIAL" , selectedPrefix) => {

  const createCondition = useCallback(() => ({
    id: generateUUID(),
    field: defaultField,
    operator: ">=",
    cctype: "017",
    index: 0,
    maxIndex: 0,
    values: {},
    gradespan_list: [],
    moduleID_list: [],
    inputValue: "",       // 新增輸入值
    isDropdownOpen: false,// 新增下拉開關
    filteredOptions: [],   // 新增條件專屬下拉資料
    visibleCount: pre_view_count // 初始只顯示電芯的筆數
  }), [defaultField]);

  const [conditions, setConditions] = useState([]);
  const [modleall_cc1, setModleAll_CC1] = useState([]); // 全部 CC1 model list
  const [modleall_cc2, setModleAll_CC2] = useState([]); // 全部 CC2 model list
  const [prevGradeMap, setPrevGradeMap] = useState({}); // { [id]: gradeValue }
  const prevGradeMapRef = useRef({}); // { [id]: index }
  const debounceMapRef = useRef({}); //每個 batch 用「自己的 debounce」
  const fetchDebounceMapRef = useRef({}); // 存每個 batch 的 debounce
  const batchRequestIdRef = useRef({});   // 存每個 batch 的 requestId (防競態)
  const prefixRef = useRef(selectedPrefix);
   // 每筆 condition 獨立 batchId
  const batchIdRef = useRef({}); 
  

  useEffect(() => {
    prefixRef.current = selectedPrefix; // 永遠保持最新值
  }, [selectedPrefix]);

  //避免舊 API 回覆蓋最新狀態
  const cancelDebounce = (id) => {
    if (fetchDebounceMapRef.current[id]) {
      fetchDebounceMapRef.current[id].cancel();
      delete fetchDebounceMapRef.current[id];
    }
  };

    // 🔁 更新某一組（通用）, 只先將最後狀態先行清除
  const updateCondition = useCallback((id, patch) => {

    setConditions(prev =>
      prev.map(c => {
        if(c.id !== id) return c;            
          let update = { ...c, ...patch };
          // 如果 cctype 被修改 → 重新 fetch
          if (patch.cctype) {
          // 清空舊資料
            update = {
              ...update,
              gradespan_list: [],
              index: 0,
              maxIndex: 0,
              values: {}              
            };

          //立即CALL , USEstate 和UI 渲染會同步差異
          fetch_Serial_MinMax_type_Digital(
            id,
            patch.cctype,
            patch.operator
          );
         }

          // index 改變 → call API SearchModelID
          if (patch.index !== undefined) {
            // const gradeValue = c.gradespan_list?.[patch.index]; 不使用此方法,因無法cancel目前最新     
            const lastnew_grades = prev.find(x => x.id === id)?.gradespan_list;
            const gradeValue = lastnew_grades?.[patch.index];
            // 清空當前batchid 指向input         
            update = {
              ...update,
              inputValue: "",
              filteredOptions: [],
              moduleID_list: [],
              isDropdownOpen: false
            };
            if (gradeValue != null && c.cctype) {
              console.log("Trigger API", id, gradeValue , patch.index);               
              //searchRef.current(id, update.cctype, gradeValue, prefixRef.current); 
              searchRef.current(id, update.cctype, gradeValue, batchIdRef.current[id]);             
            }
          }

         return update;
      }
     )
    );
  }, [selectedPrefix]);

  //useMemo 固定 instance
  const fetch_Serial_MinMax_type_Digital  = useCallback((id, cctype, operator) => {  
      console.log(
        "useEffect 觸發",
        Date.now(),
        selectedPrefix
      );
  
        if (!selectedPrefix) {
        console.warn("沒有電芯年份前綴序號!");
        return;
      }
      
      // cancel 舊 debounce
      cancelDebounce(id);

       // 每個 batch 建立 debounce
      const requestId = Date.now();
      batchRequestIdRef.current[id] = requestId;

        // 新 batchId（針對這筆 condition）
      const batchId = Date.now();
      batchIdRef.current[id] = batchId;

      
      fetchDebounceMapRef.current[id] = debounce(async () => {          
          try {      
              const response = await axios.get(
                //"http://localhost:3009/scatterdigram/get_serial_Digital",
                `${config.apiBaseUrl}/scatterdigram/get_serial_Digital`,                     
                {
                    params: {
                      s_number : (id),
                      serial_number: selectedPrefix, 
                      cc_type : cctype ,
                      op_mode: operator
                    },
                }
              );
    
              // console.log(" Serial_MinMax_type_Digital 回饋 Data = ", response.data);
              console.log(JSON.stringify(response.data, null, 2));

              //目前此batch提交已經不是最新筆，直接 return
              if (batchRequestIdRef.current[id] !== requestId) return;
    
              const index_list =  Object.values(response.data?.positive_data || []);
              const firstGrade = index_list[0];

              // sync values
              // updateCondition(id, {
              //   values: response?.data ||{},
              //   maxIndex: index_list.length || 0,
              //   gradespan_list : index_list        
              // });

              setConditions(prev =>
                prev.map(c =>
                  c.id === id
                    ? {
                        ...c,
                        values: response?.data ||{},
                        maxIndex: index_list.length || 0,
                        gradespan_list: index_list                      
                      }
                    : c
                )
              );

              //當級距有搜尋到且為數值狀態
              if (firstGrade != null && !isNaN(firstGrade)) { 
                // console.log("當級距有搜尋到(狀態,第一筆)為:"+ index_list[0]);
                //直接 call ,直接呼叫 SearchModelID，使用最新 prefix                                              
                //searchRef.current(id, cctype, firstGrade, prefixRef.current);
                searchRef.current(id, cctype, firstGrade, batchId);
                
              }
              // const access_token = response.data.access_token;
              // console.log("交換 token 成功", access_token);
        } catch (error) {
              console.error("交換 response.data 失敗", error);
        }  
      }, 500);
    
    // fetchDebounceMapRef.current[id](id, cctype, operator); 
    fetchDebounceMapRef.current[id](); // 立即觸發              
   },
    [selectedPrefix]
  );

   // debounce API for model_id search
   const SearchModelID = useCallback( async (id, cctype, gradeValue, batchId) => {

      if (!selectedPrefix || !cctype || gradeValue === undefined) return;

      const request_param = {
        s_number: id,
        serial_number: selectedPrefix,
        cc_type: cctype,
        grade_span: gradeValue,
        batchId : batchId
      };

      try {

      const response = await axios.post(
        //"http://localhost:3009/scatterdigram/get_prefix_modelID_name",
        `${config.apiBaseUrl}/scatterdigram/get_prefix_modelID_name`, 
        request_param,
         {
              headers: {
                "Content-Type": "application/json"
              },
        });


        console.log(" SearchModelID 回饋 Data = ", response.data);

        setConditions(prev =>
           prev.map(c => {
              if (c.id !== id) return c;
              const newList = response.data?.allmodleID || [];
              return {
                ...c,
                moduleID_list: newList,        // ✅ 更新資料來源
                filteredOptions: newList,      // ✅ 同步 dropdown
                visibleCount: pre_view_count,  // ✅ 重置可見數量
                isDropdownOpen: newList.length > 0
              };
           })
        );
       
    } catch (err) {
      console.error(err);
    }
  },
    [selectedPrefix]
  );

  //查詢實際條件之電芯模組名單
  const handleInputChange = useCallback(async(e, conditionId) => {
    const modleId_name = e.target.value;

    // 先更新對應 conditionId 的 inputValue
     setConditions(prev =>
      prev.map(c => {
        
        if (c.id !== conditionId) {
          // ❗ 關閉其他 dropdown
          return {
            ...c,
            isDropdownOpen: false
          };
        }

         // toggle 自己
        if (c.isDropdownOpen) {
          return {
            ...c,
            isDropdownOpen: false
          };
        }
        
        let filtered = [];

        if (modleId_name.trim() === "") {
          // input 空白 → 顯示所有
          filtered = [...c.moduleID_list];
        } else {
          filtered = c.moduleID_list.filter(model =>
            model.toUpperCase().includes(modleId_name.toUpperCase())
          );
        }

      return {
        ...c,
        inputValue: modleId_name.trim(),
        filteredOptions: filtered,
        isDropdownOpen: filtered.length > 0,
        visibleCount: pre_view_count // 重置可見筆數
      };
     })
   );

  }, [selectedPrefix])

  const handleFocusShowAll = useCallback(async(conditionId) => {
    setConditions(prev =>
      prev.map(c => {        
         if (c.id !== conditionId) {
          // ❗ 關閉其他 dropdown
          return {
            ...c,
            isDropdownOpen: false
          };
        }

         // toggle 自己
        if (c.isDropdownOpen) {
          return {
            ...c,
            isDropdownOpen: false
          };
        }

        // ✅ 只在 input 為空時才顯示 dropdown
        if (!c.inputValue || c.inputValue.trim() === "") {
          const allList = c.moduleID_list || [];
          return {
            ...c,
            filteredOptions: allList,
            isDropdownOpen: allList.length > 0,
            visibleCount: pre_view_count
          };
        }

        // input 不為空 → 不做任何事
        return c;
      })
    );
  }, [selectedPrefix])



  const  handle_onScroll_control = useCallback(async(e ,conditionId) => {
      const target = e.target;
      if (target.scrollHeight - target.scrollTop === target.clientHeight) {
        // 滾到底
        setConditions(prev =>
          prev.map(cond => {
            if (cond.id !== conditionId) return cond;
            return {
              ...cond,
              visibleCount: Math.min(
                cond.visibleCount +  pre_view_count,
                cond.filteredOptions.length
              )
            };
          })
        );
      }

  }, [selectedPrefix]);

  const handleOptionSelect = useCallback(async(conditionId, option) => {
    setConditions(prev =>
      prev.map(c => {
        if (c.id !== conditionId) return c;
        return {
          ...c,
          inputValue: option,
          isDropdownOpen: false,
          filteredOptions: [],
          visibleCount: pre_view_count
        };
      })
    );
  }, [selectedPrefix]);
  

  const searchRef = useRef(SearchModelID);

  // 保存函數引用
  useEffect(() => {
    searchRef.current = SearchModelID;
  }, [SearchModelID]);

  //debounce 只建立一次,不會 render 重建 ,防止重複送出api request 需求
  const debouncedFetchModelID = (id) => {
    if (!debounceMapRef.current[id]) {
      debounceMapRef.current[id] = debounce((cctype, gradeValue) => {  
        SearchModelID(id, cctype, gradeValue);
      }, 500);
    }
    return debounceMapRef.current[id];
  };


   //cctype 改變 →  fetch_Serial_MinMax_type_Digital
  //  useEffect(() => {
  //   conditions.forEach(c => {
  //     if (c.cctype) {
  //       fetch_Serial_MinMax_type_Digital(c.id, c.cctype, c.operator);
  //     }
  //   });

  // }, [conditions.map(c => c.cctype).join(",")]);

  
  //gradespan[c.id]的索引值 改變 → SearchModelID
//   useEffect(() => {
//    conditions.forEach(c => {

//     const gradeValue = c.gradespan_list[c.index];

//     if (gradeValue === undefined || gradeValue === null || !c.cctype) return;
    
//     const prevValue = prevGradeMapRef.current[c.id];

//     if (prevValue === gradeValue) return;

//     // 只有當 c.id 指向 prevValue 改變才呼叫
   
//       console.log("Trigger API", c.id, gradeValue);
//       prevGradeMapRef.current[c.id] = gradeValue; 
//       debouncedFetchModelID(c.id, c.cctype, gradeValue); // 確保傳入最新值             
//   });
// }, [ conditions.map(c => `${c.id}-${c.index}-${c.gradespan_list?.[c.index]}`).join("|"),
//   debouncedFetchModelID]);

  // ➕ 新增
  const addCondition = useCallback(() => {
    const new_setoption = createCondition();

    setConditions(prev => [...prev, new_setoption]);

     // 立即呼叫 API，鎖定這個條件的 id
    fetch_Serial_MinMax_type_Digital(
      new_setoption.id,
      new_setoption.cctype,
      new_setoption.operator
    );
  }, [fetch_Serial_MinMax_type_Digital]);

  // ➖ 刪除指定 id
  const removeCondition = useCallback((id) => {
    setConditions(prev => {
      if (prev.length === 1) {
        return [createCondition()];
      }
      return prev.filter(c => c.id !== id);
    });
  }, []);



  // 重置條件（prefix 改變時使用）
  const resetConditions = useCallback(() => setConditions([]), []);

  // 📦 組 backend query
  const buildQuery = useCallback(() => {
    const params = new URLSearchParams();
    

    conditions.forEach(c=> {
      if (!c.values.length) return;

      params.append(`${c.field}_op`, c.operator);
      params.append(c.field, c.values[c.index]);
      params.append(`${c.field}_grade`, c.gradespan_list?.[c.index] ?? 0);
      
  });

    return params.toString();
  }, [conditions]);


  useEffect(() => {
    if (!Array.isArray(conditions)) return; // 安全檢查
    setModleAll_CC1([]);
    setModleAll_CC2([]);
    const allModle_CC1_info =[];
    const allModle_CC2_info =[];
    //const allModleValues = conditions.map(c => c?.inputValue || "");
     conditions.map((item ,idx) =>{
      //依據cctype 和 電芯號 做判斷分別存取
      const type_set = item?.cctype;     
      type_set !==undefined && type_set.toString().includes("010")? allModle_CC1_info.push({cc_type:type_set, modle_name:item.inputValue}):allModle_CC2_info.push({cc_type:type_set, modle_name:item.inputValue});
    })

    //存入追蹤後續api查詢
    setModleAll_CC1(allModle_CC1_info);
    setModleAll_CC2(allModle_CC2_info);


    // console.log("allModleValues 是否為陣列 ->", Array.isArray(allModleValues), allModleValues);
    // setModleAllName(allModleValues);
  }, [conditions]);

  return {
    conditions,
    // modleallname,
    modleall_cc1,
    modleall_cc2,
    addCondition,
    removeCondition,
    updateCondition,
    fetch_Serial_MinMax_type_Digital,
    resetConditions,
    handleInputChange,
    handleFocusShowAll,
    handle_onScroll_control,
    handleOptionSelect,
    buildQuery
  };
};
