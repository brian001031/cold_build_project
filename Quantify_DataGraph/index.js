import "./index.scss";
import debounce from "lodash/debounce";
import React, { useState, useEffect, useRef, useMemo ,useCallback } from "react";
import { createPortal } from 'react-dom';
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Form from "react-bootstrap/Form";
// eslint-disable-next-line no-unused-vars
import config from "../../config";
import * as echarts from "echarts/core";
import { Advancedselect_trigger } from "../../components/AdvancedSelectTrigger";
import { isArray } from "lodash";
import { FormattedMessage, useIntl } from "react-intl";
// 導入 MessagePopup 組件
import MessagePopup from '../../components/MessagePopup';
	
// import { Space } from "lucide-react";
// import { Input } from "reactstrap";
// import Button from "react-bootstrap/Button";
// import { Select } from "antd";

// const cc_list = ["CC1","CC2"]
const cc_list = ["017","010"]


function Quantify_data_graph() {
  const [serial_prefixlist, setSerial_prefixlist] = useState([]);
  const [selected_serial, setSelected_serial] = useState("");
  const [selected_cc_type, setSelected_CC_Type] = useState(cc_list[0]);
  const [sideoption, setSideoption] = useState("Sulting"); //預設32分選站別
  const [inputs, setInputs] = useState([]);
  const [sqlMaxValue, setSqlMaxValue] = useState(10); // 假設從 SQL 取得最大值為 10  
  const modleIDlist_chartRef = useRef(null);
  //各電芯壓段(V2.0,V3.6,V3.5_com)電容量 ----start----------
  const [cc1_modle_capInfo, setCC1_CapacityInfo] = useState([]);  
  const [cc2_modle_capInfo, setCC2_CapacityInfo] = useState([]);
  //-------------------------end---------------------------------
  const [cc1_cap_total_array, setCC1_Cap_total_array] = useState([]);  // 各壓段CC1電容總量
  const [cc2_cap_total_array, setCC2_Cap_total_array] = useState([]);  // 各壓段CC2電容總量
  // MessagePopup 狀態管理
  const [messagePopup, setMessagePopup] = useState({
    show: false,
    type: 'info',
    title: '',
    message: ''
  });

  // 顯示訊息
  const showMessage = useCallback((type, message, title = '') => {
    setMessagePopup({
      show: true,
      type,
      title,
      message
    });
  }, []);

  // 關閉訊息
  const hideMessage = useCallback(() => {
    setMessagePopup(prev => ({ ...prev, show: false }));
  }, []);
    
  //  useRef 儲存「電芯前綴字串」
  const prevSerialRef = useRef("");
  const {
    conditions,
    // modleallname,
    modleall_cc1,
    modleall_cc2,
    addCondition,
    removeCondition,
    updateCondition,
    resetConditions,
    handleInputChange,
    handleFocusShowAll,
    handle_onScroll_control,
    handleOptionSelect,
    buildQuery
  } = Advancedselect_trigger("SERIAL" ,selected_serial);


  //下列為實現級距(bar , pie-line)圖形數據
  const chartRef = useRef(null); // 创建 ref(bar) 来引用 DOM 元素
  const chartRef2 = useRef(null); // 创建 ref2(pie-line) 来引用 DOM 元素
  const navigate = useNavigate();



 // 一開始先接收目前電芯號前綴序號別名(just do one times)
  useEffect(() => {

    if(!sideoption){
       console.error("無法擷取站點名稱 side-options:", sideoption);       
       return ;
    }
    
    //page 登入將所有電芯年份前綴先收集呈現
    const fetch_modelId_prefix_list = async (side_name) => {
      try {
            const res = await fetch(
            // `http://localhost:3009/scatterdigram/model_prefixlist?sidename=${side_name}`,
            `${config.apiBaseUrl}/scatterdigram/model_prefixlist?sidename=${side_name}`,

            );

          if (!res) throw new Error(`無擷取相關-> ${side_name}站電芯序號資訊!`);

          const result = await res.json();

          
          // console.log("目前接收 list 清單為= " + JSON.stringify(data.data,null,2));

          //清空列表單
          setSerial_prefixlist([]);
          
          if (res.status === 200 && result.data)
          {
            // console.log("接收回傳存取進行中..");            
            const serial_options = result.data.map((item, index) => ({
              prefix: item.model_prefix,
              num: index
            }));

            setSerial_prefixlist(serial_options);
          }

      } catch (error) {
          console.error("Error fetching options:", error);
          return "";
      }
   };

   fetch_modelId_prefix_list(sideoption);
   
 },[]);


  useEffect(() => {
    //  console.log("得出serial_prefixlist "+ JSON.stringify(serial_prefixlist,null,2));
    //第一次執行賦予第一筆index
    const firstPrefix = serial_prefixlist?.[0]?.prefix ||"";
    setSelected_serial( (prev) => firstPrefix+"");

  },[serial_prefixlist]);

  useEffect(() => {

    // 比對目前的值與 useRef 裡存的上一次值
    if (prevSerialRef.current !== selected_serial) {
      // console.log(`偵測到切換電芯序號字串！`);            
      resetConditions();
      //動作完成後，更新 useRef 為目前的值，供下次比對
      prevSerialRef.current = selected_serial;
    }
  }, [selected_serial]); // 當 selected_serial 改變時觸發

  const handleChange =  (e) => {
    const { name, value } = e.target;
  
    if( name === "prefix"){
      setSelected_serial(prev => value +"");      
    }
   
  };

  const IsNonOrIvaild_ModleID_count = (datalist) => {
   const total_none = datalist.filter(r => r.modle_name === "" || !r.modle_name.includes(selected_serial));
   return total_none.length;
  };

  //顯示分佈電容量的chart圖形
  const Draw_ModelId_Statisticalchart = async (e) => {
    e.preventDefault();    
    // console.log("cc1 立即整理為"+ JSON.stringify(modleall_cc1,null,2) +"結構為是否陣列: "+ Array.isArray(modleall_cc1));
    // console.log("cc2 立即整理為"+ JSON.stringify(modleall_cc2,null,2) +"結構為是否陣列 "+  Array.isArray(modleall_cc2));
     const cc1_non_count =  IsNonOrIvaild_ModleID_count(modleall_cc1);
     const cc2_non_count =  IsNonOrIvaild_ModleID_count(modleall_cc2);
     const cc1_data_len  = Object.values(modleall_cc1).length;
     const cc2_data_len  = Object.values(modleall_cc2).length;
   
    const hasCC1Error = cc1_data_len > 0 && Number(cc1_non_count) > 0;
    const hasCC2Error = cc2_data_len > 0 && Number(cc2_non_count) > 0;
 
     //當有空電芯號提示
     if(hasCC1Error || hasCC2Error){      
      showMessage('warning','有空電芯號或序號前綴錯誤可能,請確認!');      
      return;
     }

     try {
          const search_modle_all = {type1: modleall_cc1 ,type2: modleall_cc2};
          
          //清空原先紀錄
          setCC1_CapacityInfo([]);
          setCC2_CapacityInfo([]);


          const response = await axios.post(
         // "http://localhost:3009/scatterdigram/get_modle_capacity_val",
          `${config.apiBaseUrl}/scatterdigram/get_modle_capacity_val`, 
          search_modle_all,
          {
              headers: {
              "Content-Type": "application/json"
              },
          }
        );

        console.log(" Draw_ModelId_Statisticalchart 回饋 Data = ", response.data);

        const get_allmodle_info = response.data?.finallyResluts??[];
        // const CC1_Cap_amount_list = response.data?.cc_cap_total?.CC1_cap_amount?? {};
        const CC1_Cap_amount_list = Object.assign({}, response.data?.cc_cap_total?.CC1_cap_amount);
        const CC2_Cap_amount_list = Object.assign({}, response.data?.cc_cap_total?.CC2_cap_amount);

        //確保有從後端解取道電芯資訊
        if( Object.values(get_allmodle_info).length > 0){
          const cctype_modle_list = get_allmodle_info.map((item,index) => {
               const { parameter , modelId, ...rest } = item;      // 先取出 parameter , modelId，其餘放 rest
               const CC_TYPE = parameter.includes(cc_list[0])?"CC2":"CC1";
               return `${index}-${CC_TYPE}-${modelId}`; // index + "-" + 其餘 key 值連接

                // return `${num}-${Object.values(rest).join('')}`; // num + "-" + 其餘 key 值連接
          });

          const cc1_amount_all = Object.entries(CC1_Cap_amount_list)
                                .filter(([key, value]) => key.includes("VAHS"))  // 過濾 key
                                .map(([key, value]) => Number(value).toFixed(2));  // 取鍵名的值value
          
          const cc2_amount_all = Object.entries(CC2_Cap_amount_list)
                                .filter(([key, value]) => key.includes("VAHS"))  // 過濾 key
                                .map(([key, value]) => Number(value).toFixed(2));  // 取鍵名的值value
          

           console.log("重整理電芯排序為: "+cctype_modle_list+'\r\n'+"cc1_未分選電容總和清單_all = "+ cc1_amount_all + '\r\n'+ "cc2_32分選電容總和清單_all = "+ cc2_amount_all);         
           
        };


   

    } catch (err) {
      console.error(err);
    }

  }


  return (
    <>
    <div className="quantify_data_graph">
       
      <header className="title_name_header">電芯品質管控採樣統計圖</header>  

      <div className="main_layout"> 

        {/* 左側 Filter Panel */}
        <aside className="filter-panel">
          <div>
            <label className="serial_label_setting">
                電芯前綴:
            </label> 
              <select className="serialselect"
                    name="prefix"
                    value={selected_serial}
                    onChange={handleChange}            
                    required
              >
                {serial_prefixlist.map((item ,index) => (
                    <option key={item.num} value={item.prefix}>{item.prefix}</option>
                ))}
              </select>
               <button class="button" onClick={(e) => Draw_ModelId_Statisticalchart(e)}>
                 電容量分布
              </button>
            </div>
            <br></br>
            <div className="condition-list">
              {conditions.map((c, idx) => (
              <div
                  key={c.id}
                  className="d-flex align-items-center gap-2 mb-2"
              >
                <span>{idx + 1}.</span>
               <div className="dropdown-wrapper">
                <input
                  type="text"
                  className="form-control "            
                  style={{ width: 195 }}
                  placeholder="搜尋"
                  value={c.inputValue || ""}
                  onChange={e => handleInputChange(e, c.id)} 
                  onClick={() => handleFocusShowAll(c.id)}  // 只在空白時觸發                
                />
                
                {c.isDropdownOpen && c.filteredOptions.length > 0 &&(
                  <ul
                    className="dropdown-options"
                    style={{ maxHeight: 350, overflowY: "auto" }}
                    onScroll={e => handle_onScroll_control(e,c.id)}
                  >
                    {c.filteredOptions.slice(0, c.visibleCount).map((option, index) => (
                      <li
                        key={index}
                        className="dropdown-option"
                        onClick={() => handleOptionSelect(c.id, option)}
                      >
                        {option}
                      </li>
                    ))}
                  </ul>
                 )
                } 
                </div>          
                <label className="type_select">
                分容：
                <select             
                    style={{ width: 70 }}
                    value={c.cctype}
                    onChange={e =>
                      updateCondition(c.id, { cctype: e.target.value })                
                    }
                >
                  
                  {/* <option value="CC2">≥</option>
                  <option value="CC1">≤</option> */}
                  { cc_list.length > 0 && cc_list.map((item , index) => 
                  (
                    <option key={index} value={item}>
                      {item.includes("010") ? "CC1" : ""}
                      {item.includes("017") ? "CC2" : ""}
                    </option>
                  ))}
                </select>
                </label>

                {/* <select
                    className="form-select"
                    style={{ width: 90 }}
                    value={c.operator}
                    onChange={e =>
                      updateCondition(c.id, { operator: e.target.value })
                    }
                >
                  <option value=">=">≥</option>
                  <option value="<=">≤</option>
                </select> */}
                <label className="positive_select">
                  級距:
                  <select
                    className="form-select"
                    style={{ width: 130 , paddingRight:"0.5em"}}
                    value={c.index}
                    onChange={e => updateCondition(c.id, { index: Number(e.target.value) })}
                  >
                    {Array.from({ length: c.gradespan_list?.length ||0}, (_, i) => (
                      <option key={i} value={i}>{c.gradespan_list[i]}</option>               
                    ))}
                  </select>
                </label>

                {/* Remove */}
                <button
                  type="button"
                  className="btn btn-outline-danger"
                  onClick={() => removeCondition(c.id)}
                >
                  −
                </button>
              </div>
            ))}

            <button type="button" 
                    className="btn btn-primary"
                    onClick={addCondition}>
            +
            </button>                 
            <pre>{buildQuery()}</pre>
          </div>
        </aside>
        {/* 右側 Chart Panel */}
        <main className="chart-panel">
          <div ref={modleIDlist_chartRef} className="chart-container"></div>
          <div>
          {(Array.isArray(modleall_cc1) ? modleall_cc1 : []).map((row , idx) =>             
            <div key={idx} style={{backgroundColor:"#FFF4C1" , fontSize:"30px"}}>
              <span>{`CC1未分選輸入-> ${idx + 1} ${JSON.stringify(row,null,2)}`}</span>          
            </div> 
            )
          }
          </div>
          <div>
          {(Array.isArray(modleall_cc2) ? modleall_cc2 : []).map((row , idx) =>             
            <div key={idx} style={{backgroundColor:"#c1fffc" , fontSize:"30px"}}>
              <span>{`CC2已分選輸入-> ${idx + 1} ${JSON.stringify(row,null,2)}`}</span>          
            </div> 
            )
          }
          </div>
        </main>
      </div>
      {/* MessagePopup 組件 */}
      <MessagePopup
        show={messagePopup.show}
        type={messagePopup.type}
        title={messagePopup.title}
        message={messagePopup.message}
        onHide={hideMessage}
        autoClose={messagePopup.type === 'success'}
        autoCloseDelay={3000}
      />
     </div>
    </>
    
  );

}



export default Quantify_data_graph;

