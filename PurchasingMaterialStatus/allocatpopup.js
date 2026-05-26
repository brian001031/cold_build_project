import React, { useState, useEffect, useRef, useCallback } from "react";
import { json, Route } from "react-router-dom";
import { Modal, Button, Card, Row, Col, Table, FormControl, Toast } from 'react-bootstrap';
import Form from "react-bootstrap/Form";
import axios from "axios";
import moment from "moment";
import 'moment/locale/zh-tw'; 
import { isArray, kebabCase } from "lodash";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { FormattedMessage, useIntl } from "react-intl";
import dayjs from "dayjs";
//成功提示套件
import { toast } from "react-toastify";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";


import config from "../../config";
import './index_allocat.scss';


const allocate_info_key = [ "採購單號","工作序" , "物料名" , "編碼","數量","單位"];

const ONE_DAY_MILSEC = 24 * 3600 * 1000   // 1個小時總毫秒數量

//初始化空數據組
const allocateRow = {
  date_stage_code: '', //分配包裝辨識週期字串
  weight: '',        // 分配重量   
};


function AllocationPopup_Work({ show, onHide,allocat_data }) {
  const [canviewA_allocate, setCanViewAllocate] = useState(false);   //判定units 若是PCS計量需要自訂義單位重量
  const [enable_radiomode, setEnable_RadioMode] = useState(false);
  const [dataRows, setDataRows] = useState([ { ...allocateRow } ]); // 儲存多筆物料重量資料
  const [allocatecase, setSelectedAllocateCase] = useState({
    prorated_method: "",
    manual_method: "",
  });

  const [radiomethod, setRadioMethod] = useState(""); // 用於儲存選擇的物料分配類型
  const now = new Date();
  const nowyear = now.getFullYear();
  const Current_date = moment(now, 'yyyy-MM-dd');
  const [first_yeardate, setFirst_YearDate] = useState(
    dayjs(new Date(new Date().getFullYear(), 0, 1)) // 預設為當年1月1日
  );
  const dayOfFirstDate = first_yeardate.day(); //取當年元旦1号是星期幾
  const [allocstage_calculate, setAllocstage_Calculate] = useState(
    dayjs().subtract(0, "day").format("YYYY-MM-DD") // 預設,目前只能擷取最新前日
  );
  
  // const key_prefix_purchstr = String(Object.values(allocat_data)[0]).slice(5);
  // console.log("接收allocat_data 資料型態為: "+ typeof allocat_data +  "前綴單號字串為:" + String(key_prefix_purchstr));

  !Array.isArray(allocat_data)?console.log("接收allocat_data 資料內容為: "+ JSON.stringify(allocat_data,null,2))
                              :console.log("接收allocat_data 資料內容List為: "+ Object.values( allocat_data));

  const adjust_unit_refix = Object.values(allocat_data)[Object.values(allocat_data).length-1] === "pcs";
  
  // console.log("目前年是: "+ nowyear);
  // console.log("今年第一天 是禮拜 "+ dayOfFirstDate);
  console.log("是否要重新定義物料重量 "+ adjust_unit_refix);

 const formatDate_local = (date) => {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, "0");
    const d = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
 };

 //找回當前年份第一週的禮拜一日期! 需要抓到該年1月1號回推天數-7
 const getISOWeekStart = (year) => {
  const jan1 = new Date(year, 0, 4);
  const day = jan1.getDay() || 7; //取的天數
   // 找到 week1 的星期一
   jan1.setDate(jan1.getDate() - day + 1);
   return jan1;
 }


 //每年的第一個星期四作為起始日期，指定日期所在周的星期四作為終止日期，再將兩者的差值除以7，即為所求週數 
 //還有一個細節就是跨年問題，指定日期有可能是在上一年的最後一個星期                            
 const getWeekOfYear = (dateStr) => {
   //當前指定日期
   const dateObj = new Date(dateStr)

   // 移到本週星期四
   dateObj.setDate(
      dateObj.getDate() + 4 - (dateObj.getDay() || 7)
   );

   dateObj.setHours(0,0,0,0);
    // const startOfYear = new Date(dateStr)
    // startOfYear.setMonth(0)//1月
    // startOfYear.setDate(1)//1日
    
    //開始先歸零 時 分 秒 毫秒 (誤差)
    // 取得該日期年份
    const currentYear = dateObj.getFullYear();

    // 動態建立該年 1/1
    const firstYearDate = new Date(currentYear, 0, 1);
    const dynmaic_getday = firstYearDate.getDay();   
    firstYearDate.setHours(0,0,0,0);

    console.log(`選擇動態年為:${firstYearDate} -> 第一天是禮拜:`+ dynmaic_getday);

    let dateOfFirstThursday //所在年份的第一個星期四的日期
    //計算所在年份的第一個星期四 (使用誤差量 minimum second 準確差異計算)
    if(dynmaic_getday < 5) { 
      //1月1日在星期五之前，则再過(4 - dayOfFirstDate)天是星期四
      dateOfFirstThursday =  firstYearDate.valueOf() + ONE_DAY_MILSEC  * (4 - dynmaic_getday)
    }else{
      //否则顺延一周，再过( 4 - dayOfFirstDate + 7) 天 才是該年的第一個星期四
      dateOfFirstThursday = firstYearDate.valueOf() + ONE_DAY_MILSEC *  ( 4 - dynmaic_getday + 7)
    }
  
    //當前日期所指向的禮拜四
    let curThursday = dateObj.getTime() + ONE_DAY_MILSEC * (4 - (dateObj.getDay() || 7))  //给定日期所在這週的星期四

    let setstage ="";
    if(curThursday >= dateOfFirstThursday) {       
      // setstage = ((curThursday - dateOfFirstThursday) / ONE_DAY_MILSEC / 7 + 1).toFixed(0)
      //符合 ISO week
      setstage = Math.floor((curThursday - dateOfFirstThursday)/ ONE_DAY_MILSEC/ 7) + 1;
    }else{
      //指定日期是在上一年的最後一個星期
      let lastDayOfLastYear = firstYearDate.valueOf() - ONE_DAY_MILSEC      
      setstage = getWeekOfYear(lastDayOfLastYear)
    }
    return setstage;
 }


    // 監聽 Radio或日期 Button 切換
  const handle_Change = async (e) => {
    const { name, value } = e.target;

    if (name === "prorated") {
      setSelectedAllocateCase({ ...allocatecase, [name]: "prorated" });
	    setRadioMethod("prorated");
      setEnable_RadioMode(true);
    } else if (name === "mannul") {
      setSelectedAllocateCase({ ...allocatecase, [name]: "mannul" });
      setRadioMethod("mannul");
      setEnable_RadioMode(true);
    } else if(name === "trip-start"){      
      setAllocstage_Calculate(value);
    }    

    //清除既有的分配欄位及對應當前選的
    //clear_item_select();
  };          

  //切換日期算出實際週期
  useEffect(() => {
    
    console.log(`日期:${allocstage_calculate} 計算為第`+getWeekOfYear(allocstage_calculate) + "週");
    
  }, [allocstage_calculate]);
  
  const handleCancel = () => {

     onHide();
  };
  
  return(
    <Modal show={show} onHide={onHide} dialogClassName="allocatetion_form">
         <Form.Group className="mb-3">              
        </Form.Group>      
        <div style={{  display:"flex",
                      flexDirection:"column",
                      padding:"15px",
                      gap:"25px"
                    }}>
                      <div style={{  display: "flex",
                          height: 50,                                                                                                                
                          width: "100%",
                          paddingLeft:"120px"                          
                        }}>                                    
                        <h1 style={{ textAlign: "center", verticalAlign: "middle",fontSize: "36px"}}>採購物料分配|Material Procurement Allocation</h1>                    
                        <button
                          type="button"
                          style={{ marginLeft: "70px", backgroundColor: "red" , alignItems:"center" }}
                          onClick={handleCancel}
                        >
                          關閉
                        </button>
                      </div>
                    <div
                      style={{
                        display:"flex",
                        flexDirection:"row",
                        gap:"5px",
                        alignItems:"flex-start"
                      }}
                    >
                    <span
                      style={{
                        fontSize: "18px",
                        marginBottom: 2,
                        color: "#0b044b",
                        borderBottom: "5px solid #555",
                        paddingBottom: 10,
                        width: 950,
                        fontWeight:"bold"
                      }}
                    >
                      處理單號 :
                      <span
                        style={{
                          fontSize: "20px",
                          backgroundColor:"#ffe066",
                          padding:"4px 10px",
                          borderRadius:"8px",
                          marginLeft:"10px",
                          color:"#7a1f1f",
                           display:"inline-block",
                          maxWidth:"120%",
                          wordBreak:"break-all",
                          whiteSpace:"normal",
                          lineHeight:"1.5"
                        }}
                      >
                        {Object.values(allocat_data)[0]}
                      </span>
                    </span>                         						      
                 {
                    allocat_data
                      .slice(1)
                      .map((it, idx) => (
                        <div
                          key={idx}
                          style={{
                            marginTop: 2,
                            padding: 10,
                            marginBottom: 50,
                            background:"rgb(240, 240, 239)",
                            borderRadius: 8,
                            border: "1px solid #0e0202",
                            width:"380px",
                            fontSize:"16px"
                          }}
                        >                         
                            <strong
                              style={{
                                color:"#0b044b"
                              }}
                            >
                              {allocate_info_key[idx + 1]}
                            </strong>
                            :
                            <span
                              style={{
                                marginLeft:3,
                                color:"#7a1f1f",
                                fontWeight:"bold"
                              }}
                            >
                              {it}
                            </span>                          
                        </div>
                      ))
                  }
                  
              </div>               
              <div className="radio-container">
                <label className="radio-item">
                  <input
                    type="radio"
                    name="prorated"
                    value={allocatecase.prorated_method}
                    checked={radiomethod === "prorated"}
                    onChange={handle_Change}
                  />
                  <p className="dbselect">平均分配</p>
                </label>
                <label className="radio-item">
                  <input
                    type="radio"
                    name="mannul"
                    value={allocatecase.manual_method}
                    checked={radiomethod === "mannul"}
                    onChange={handle_Change}
                  />
                  <p className="dbselect">手動自行分配</p>
                </label>
              </div> 
        </div> 
        {/*當選擇好分配模式,顯示以下元件*/}
        { enable_radiomode  && 
            <div>
            <label htmlFor="start">選擇配料入倉週期(日期選):</label>
            <input
              type="date"
              id="start"
              name="trip-start"
              min={dayjs(getISOWeekStart(nowyear)).format("YYYY-MM-DD")}                
              value={allocstage_calculate}
              // 🔒 禁止手動輸入 ,避免輸入1日期預設為到該年第一天導致資料量max
              onKeyDown={(e) => e.preventDefault()}         
              onChange={handle_Change}
            />                 
          </div>
        }           
     </Modal>
  );


}

export default AllocationPopup_Work;