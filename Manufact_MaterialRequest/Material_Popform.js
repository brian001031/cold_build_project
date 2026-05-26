import React, { useState, useEffect, useCallback, useRef } from "react";
import debounce from "lodash.debounce";
import config from "../../config";
import { Form, Button } from "react-bootstrap";
import Table from "react-bootstrap/Table";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import dayjs from "dayjs";
import "./material_popform.scss";
//成功提示套件
import { toast } from "react-toastify";

const Material_Popform = ({ show , item_prefixkey , onHide}) => {
  const [quality_prefixsource, setquality_prefixsource] = useState([]);
  const [quality_list_group, setquality_list_group] = useState([]);
  const [openGroup, setOpenGroup] = useState({});
  // const [filenameList, setFileNameList] = useState([
  //   {
  //     queryfile: "",
  //     isManual: true,
  //     mode: "input",
  //     candidates: [],
  //     originalCandidates: [],
  //   },
  // ]);
  const [visibleRows, setVisibleRows] = useState(20); // Initially show 20 rows
  const [popvalues, setpopValues] = useState({
    ID_head: "",
    ID_end: "",
  });

  const latestQueryRef = useRef(null);


  //  包裝 debounce function，確保不重建
  // eslint-disable-next-line react-hooks/exhaustive-deps
  // const fetchPFCC_FileName = useCallback(
  //   debounce(async (KeyFilelist, index) => {
  //     // console.log("PFCC搜尋list =", JSON.stringify(KeyFilelist, null, 2));
  //     console.log(" 查詢 index:", index, "→", KeyFilelist);
  //     if (!KeyFilelist || !KeyFilelist.queryfile) return;
  //     try {
  //       const response = await axios.get(
  //         `${config.apiBaseUrl}/purchsaleinvtory/query_FileName`,
  //         // "http://localhost:3009/purchsaleinvtory/query_FileName",
  //         {
  //           params: {
  //             RadioValue,
  //             Rawtable: FormRawtable,
  //             FileName_titleKey: KeyFilelist,
  //           },
  //         }
  //       );

  //       if (response.status === 200) {
  //         // console.log("回傳成功:", response.data);

  //         // const converted = response.data.map((item) => ({
  //         //   queryfile: item.FileName,
  //         //   isManual: false,
  //         // }));

  //         const converted = response.data.map((item) => item.FileName);

  //         // setFileNameList((prevList) => {
  //         //   const newList = [...prevList];
  //         //   // 可選：若你想保留原始手動輸入那一筆
  //         //   const manualInput = { ...newList[index] };
  //         //   newList.splice(index, 1, ...converted);
  //         //   // 移除 index 那一筆，插入多筆新資料（+可選手動那筆）,不想保留原手動輸入那筆，就把 manualInput 拿掉，
  //         //   // newList.slice(index, 1, manualInput, ...converted);
  //         //   // newList.splice(index + 1, 0, ...converted); // 插入在當前輸入欄位後
  //         //   // newList[index] = { queryfile: converted[0]?.queryfile || "" };
  //         //   //return newList.slice(0, 14); // 限制最多 14 筆
  //         //   return newList;
  //         // });

  //         setFileNameList((prevList) => {
  //           const newList = [...prevList];

  //           // 保險檢查，防止 index 不存在時 set 錯
  //           if (!newList[index]) return newList;

  //           newList[index] = {
  //             ...newList[index],
  //             candidates: converted,
  //             //queryfile: converted[0] || "", // 預設第一筆為選中值
  //             // queryfile: "", // 不預選
  //             // isManual: false,
  //             queryfile: newList[index].queryfile || "", // 如果已有值就保留
  //             isManual: newList[index].queryfile
  //               ? newList[index].isManual
  //               : false,
  //             mode: "select", // 初始顯示 dropdown
  //           };
  //           return newList;
  //         });

  //         // setFileNameList((prevList) => [...prevList, ...converted]);
  //         //setFileNameList(converted);
  //         //確定要傳到後端搜尋
  //         setSendFileName_search(true);
  //       }
  //     } catch (err) {
  //       console.error("API 錯誤:", err);
  //     }
  //   }, 300),
  //   [RadioValue, FormRawtable]
  // );

  const isPositiveInteger = (value) => {
    const num = Number(value);
    return Number.isInteger(num) && num > 0;
  };

  useEffect(() => {
    //先行判定字元是否為有效(Eng字母)
    if(String(item_prefixkey) ===''){
      console.log("目前為空字元,NG! -> "+ item_prefixkey)
      return
    }

    const Get_Qualityassurancelist_Item = async (e) => {
        try {
          const res = await axios.get(
            `${config.apiBaseUrl}/purchsaleinvtory/purchase_online_wavehouse_item`,
            // "http://localhost:3009/purchsaleinvtory/purchase_online_wavehouse_item",
            {
              params: {
                prefix_charactor:  item_prefixkey ?? 'A',     //預設'A'字元開頭     
              },
            }
          );

          const quality_allData = res.data.rowdatas;
        
          if (res.status === 200 && Object.values(quality_allData).length > 0) {
              console.log(
                "有接收Get_Qualityassurancelist_Item 回傳data = " +
                  JSON.stringify(quality_allData, null, 2)
              );
                       
             setquality_prefixsource(quality_allData);                  
          }
        } catch (err) {
          console.error("Error fetching Get_Qualityassurancelist_Item api protocal:", err);
        } finally {
        }
      };

      Get_Qualityassurancelist_Item();

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);


  useEffect(() => {
    
  //groupBy 前兩碼
  const groupedData = quality_prefixsource.reduce((acc, item) => {
    const groupKey = item.it_type.substring(0, 2);
      if (!acc[groupKey]) {
        acc[groupKey] = [];
      }
      acc[groupKey].push(item);
      return acc;
  }, {});


  //排序
  Object.keys(groupedData).forEach((key) => {
      groupedData[key].sort((a, b) => {
        const numA = parseInt(a.it_type.split("-")[1]);
        const numB = parseInt(b.it_type.split("-")[1]);
        return numA - numB;
      });
  });

  setquality_list_group(groupedData);


}, [quality_prefixsource]);

//將group 選項重整群組分類
const toggleGroup = (group) => {
  setOpenGroup(prev => ({
    ...prev,
    [group]: !prev[group]
  }));
};


  // 取消按鈕的處理函數
  const handleBack = () => {
   onHide();
  };

  // const handleScroll = (e) => {
  //   // console.log(
  //   //   e.target.scrollHeight,
  //   //   e.target.scrollTop,
  //   //   e.target.clientHeight
  //   // );

  //   const bottom =
  //     e.target.scrollTop + 5 + e.target.clientHeight >= e.target.scrollHeight;

  //   if (bottom) {
  //     console.log("Reached the bottom");
  //     setVisibleRows((prevVisibleRows) => prevVisibleRows + 20); // Load next 20 rows
  //   }
  // };


  // Handle form submission
  const handleSubmit = (e) => {
    e.preventDefault();
    // console.log("Form Data Submitted:", FormRawtable);
    //清空flag
    // have_exist_ID(false);
    // setPFCC1andCC2(false);
   // onHide(); // Close the modal after submission
  };

  return (
    <div className="material_popform_display">
      <form onSubmit={handleSubmit}>
        <div className="modal-overlay"> 
           <div className="toolbar">
            <Button variant="primary" style={{width:"200px" , color:"rgb(228, 185, 68)", background:"rgb(27, 62, 219)" , marginBottom:"20px", fontSize:"20px"}} onClick={handleBack}>
              ← 回主畫面
            </Button>
          </div>        
           <div className="accordion-container">                      
            {Object.keys(quality_list_group).sort().map((group) => (       
               // {/*當有section 觸動A開頭選單*/}                                        
              <div key={group} className={`group-section ${openGroup[group]? "active": ""}`}>
                {/* Header */}
                <div
                  className="group-header"
                  onClick={() => toggleGroup(group)}
                >
                  {group}
                  <span>
                    {openGroup[group] ? "▲" : "▼"}
                  </span>
                </div>
                {/* Content */}
                {openGroup[group] && (
                  <div className="group-content">
                    {quality_list_group[group].map((item) => (
                      <div
                        key={item.it_type}
                        className="material-card"
                      >
                        <span className="material-title">物料明細▽</span>
                          <div className="material-info">                            
                            <div className="info-row">
                                <span className="label">編碼</span>
                                <span className="value">{item.it_type}</span>
                              </div>

                              <div className="info-row">
                                <span className="label">名稱</span>
                                <span className="value">{item.itemName}</span>
                              </div>

                              <div className="info-row">
                                <span className="label">規格</span>
                                <span className="value">{item.specification}</span>
                              </div>

                              <div className="info-row">
                                <span className="label">供應商</span>
                                <span className="value">{item.Vendor}</span>
                              </div>
                          </div>
                      </div>
                    ))}
                  </div>
                )}
               </div>        
            ))}
            
          </div>
        </div>
      </form>
    </div>
  );
};

export default Material_Popform;
