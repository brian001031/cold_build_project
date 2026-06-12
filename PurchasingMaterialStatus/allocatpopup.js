import React, { useState, useEffect, useRef, useCallback } from "react";
import { json, Route } from "react-router-dom";
import { Modal, Button, Card, Row, Col, Table, FormControl, Toast } from 'react-bootstrap';
import Form from "react-bootstrap/Form";
import axios from "axios";
import moment from "moment";
import 'moment/locale/zh-tw'; 
import { isArray, kebabCase, lowerCase } from "lodash";
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
import { number } from "echarts";
import { NonBinaryIcon } from "lucide-react";
 


const allocate_info_key = [ "採購單號","工作序" , "物料名" , "編碼","數量","單位","廠商碼"];

const ONE_DAY_MILSEC = 24 * 3600 * 1000   // 1個小時總毫秒數量

const unit_options = [
  { val: "g", type: "公克" },
  { val: "kg", type: "公斤" },
  { val: "cm", type: "公分" },
  { val: "m", type: "公尺" },
  { val: "m_2", type: "平方公尺" },
  { val: "lt", type: "公升" },  
];

const  mapping_unit_zthw_ch = {
  g: "重量",
  kg:"重量",
  cm:"長度",
  m:"長度",
  m_2:"面積",
  lt:"容量"
}

const unit_len_mapping= {
  roll: ["cm", "m", "m_2"],
};

//初始化空數據組
  const createMannulValue ={
      date_stage_code: '',  //分配包裝辨識週期字串
      inputValue: '',       // 單位量值   
  };


const not_weight_units = [ "pcs", "qty" , "roll"]

function AllocationPopup_Work({ show, onHide,allocat_data }) {
  const [allocate_baseweight, setAllocate_BaseWeight] = useState(0);   //判定units 若是PCS計量需要自訂義單位重量,預設為0公克
  const [final_weight, setAll_FinalWeight] = useState(0);
  const [confirm_lastunit, setConfirm_LastUnit] = useState("");  
  const [enable_radiomode, setEnable_RadioMode] = useState(false);
  const [allocate_need, setAllocate_select_need] = useState(false);
  const [baseuiut_type, setbaseuiut_type] = useState(unit_options[0]["val"]);   
  const [weightError, setWeightError] = useState("");
  const [inputErrors, setInputErrors] = useState({});  // 手動分配數值異常 (error紀錄顯示訊息提醒)
  const [allocate_dataRows, setallocate_DataRows] = useState([ { ...createMannulValue } ]); // 儲存多筆物料重量資料
  const [allocatecase, setSelectedAllocateCase] = useState({
    prorated_method: "",
    manual_method: "",
  });

  const [radiomethod, setRadioMethod] = useState(""); // 用於儲存選擇的物料分配類型
  const now = new Date();
  const nowyear = now.getFullYear()-1;  //這邊依據需求最早可追朔到去年
  const Current_date = moment(now, 'yyyy-MM-dd');
  const [first_yeardate, setFirst_YearDate] = useState(
    dayjs(new Date(new Date().getFullYear(), 0, 1)) // 預設為當年1月1日
  );
  const dayOfFirstDate = first_yeardate.day(); //取當年元旦1号是星期幾
  const [allocstage_calculate, setAllocstage_Calculate] = useState(
    dayjs().subtract(0, "day").format("YYYY-MM-DD") // 預設,目前只能擷取最新前日
  );
  const [float_support, setfloat_support] = useState("0"); // 用於自動分配精準度與否切換 ,預設不支援浮點數 
  const [max_packet_num, setMax_Packet_Num] = useState(1); // 用於自動分配精準度與否切換 ,預設不支援浮點數 
  const prevWeightRef = useRef(allocate_baseweight); // 監聽狀態儲存(初始化)
  
  // const key_prefix_purchstr = String(Object.values(allocat_data)[0]).slice(5);
  // console.log("接收allocat_data 資料型態為: "+ typeof allocat_data +  "前綴單號字串為:" + String(key_prefix_purchstr));

  // !Array.isArray(allocat_data)?console.log("接收allocat_data 資料內容為: "+ JSON.stringify(allocat_data,null,2))
  //                             :console.log("接收allocat_data 資料內容List為: "+ Object.values( allocat_data));

  
  const unit_fields = Object.values(allocat_data)[Object.values(allocat_data).length-2];  
  const number_request_value  = Object.values(allocat_data)[Object.values(allocat_data).length-3];   // index:4 為數量值    
  const item_encode =   Object.values(allocat_data)[Object.values(allocat_data).length-4];
  const venderid = Object.values(allocat_data)[Object.values(allocat_data).length-1];
  const adjust_unit_refix = not_weight_units.includes(lowerCase(unit_fields)) ;
  const [allocatePCS, setAllocatePCS] = useState(1 ||Math.ceil(Number(number_request_value)));   //預設為 allocate 分配為採購給予的pcs數值
  
  const g_unitText_type = adjust_unit_refix ? String(baseuiut_type) : String(unit_fields);

  // console.log("目前年是: "+ nowyear);
  // console.log("今年第一天 是禮拜 "+ dayOfFirstDate);
  console.log("是否要重新定義物料重量:"+ adjust_unit_refix);


  

  const displayUnitOptions = unit_len_mapping[lowerCase(unit_fields)]
    ? unit_options.filter((item) =>
      unit_len_mapping[lowerCase(unit_fields)].includes(item.val)
    )
  : unit_options;

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


 //找出下一筆合理入倉流水號
 const getNextSerial = () => {
    if (allocate_dataRows.length === 0) {
      return "001";
    }

    const maxNo = Math.max(
      ...allocate_dataRows.map(item => {
        const parts = item.date_stage_code?.split("-") || [];
        return Number(parts[2] || 0);
      })
    );

    return String(maxNo + 1).padStart(3, "0");
 };


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

 //平均分配
 const buildProratedRows = async( house_wave_year , stage , avg_quantity ) => {
    const intWeight = parseFloat(final_weight); //用原始浮點數

    let avg , remainder ;

    //只支援正整數分配量 ,最後未整除餘數當作最後一包
    if(float_support === "0"){
        //取商,餘數 all 整數部分
      avg = Math.floor(intWeight / avg_quantity); 
      remainder = intWeight % avg_quantity ; 

      //當avg 為 0 結果,通知操作者無法配分,並保持之前最後狀態      
      if( Number(avg) === 0 ){
          toast.error(`分配後計算單包數值為:${Number(avg)},異常,目前最多均分為->${String(max_packet_num)}包!`);
          return;
      }else{
        setMax_Packet_Num(parseInt(avg_quantity)); //儲存可容納最大包數量
      }
       
    } //支援浮點數分配量
    else{
      //取商為浮點數(需精準),每包均量
      const avg_normal = Math.fround(parseFloat(intWeight) / parseFloat(avg_quantity));
      avg =  Number((intWeight / avg_quantity).toFixed(2))
      // const remainder = intWeight % avg_quantity ;
      console.log("32-bit float (單精度浮點數) = "+ parseFloat(avg_normal) + "JavaScript Number計算為 = "+ parseFloat(avg));
    }

    const avg_row = [];
    const final_itemconde_entry = item_encode!==null ? item_encode.trim() :"??-???-???";

    for (let i = 0; i < avg_quantity; i++) {
      //目前流水號從001~999 (最多一千筆)
      const save_stage = parseInt(stage) < 10 ? String(stage).padStart(2, '0'):String(stage);
      const serial_houwave_code = final_itemconde_entry+'-'+ String(venderid) +'-' + house_wave_year+save_stage+'-'+ String(i + 1).padStart(3, '0');
      avg_row.push({
        date_stage_code: serial_houwave_code,
        // inputValue: i < avg_quantity -1 ? String(avg) : String(avg + remainder)
        inputValue: (float_support === "0")? i < avg_quantity -1 ? String(avg) : String(parseInt(avg + remainder)):String(avg)
      });
    }
    //存入自動分配紀錄
    setallocate_DataRows(avg_row);
 }

 //手動分配下列功能函數
  // 新增手動添加值
 const addMannulCfg_value= (() => {
   
    //取得當前選擇週期
    const stage_d = String(getWeekOfYear(allocstage_calculate)).trim(); 

    const save_stage =parseInt(stage_d) < 10
      ? String(stage_d).padStart(2, "0")
      : String(stage_d);

   const shift_thursday = new Date(allocstage_calculate);    // 複製一份日期避免修改原物件
    shift_thursday.setDate(shift_thursday.getDate() + 4 - (shift_thursday.getDay() || 7));  // 找到該週星期四
   
   const house_wave_year = shift_thursday.getFullYear();

   const final_itemconde_entry = item_encode!==null ? item_encode.trim() :"??-???-???";
   

   const serial_houwave_code =final_itemconde_entry +"-" + String(venderid) + '-' +house_wave_year +save_stage +"-" +String(allocate_dataRows.length + 1).padStart(3, "0");


   const newRow = {
      ...createMannulValue,
      date_stage_code: serial_houwave_code,
      inputValue: "",
    };

    //需要擷取當前年週期紀錄 
    setallocate_DataRows(prev => [
      ...prev,
      newRow
    ]);

       
    //setallocate_DataRows(prev => [...prev,{ ...createMannulValue }]);
 });

  //刪除指定id序號
 const removeMannulCfg =  ((idx) => {

    // if (allocate_dataRows.length === 0) {
    //     return [{ ...createMannulValue }];
    // }

    setallocate_DataRows(prev => {

      // console.log("刪除前", prev.map(x => x.date_stage_code));
      const newData = prev.filter((_, index) => index !== idx);
      
      // console.log("刪除後", newData.map(x => x.date_stage_code));

      //重整排序新資料列
      return newData.map((item, index) => {

        //當空值則初始化當前狀態
        if (!item.date_stage_code) {
          return item;
        }

        const parts = item.date_stage_code?.split("-") || [];

        //前綴欄位回傳(只針對編碼欄位做做小限度判斷(兩欄))
        if (parts.length < 3) {
          return item;
        }

         return {
            ...item,
            date_stage_code: [
              ...parts.slice(0, -1),
              String(index + 1).padStart(3, "0")
            ].join("-")
          };  

      });       
    });
 });

 //偵測手動輸入整體狀態,以利後續提交判定是否正常與否!
 const Mannul_Result = allocate_dataRows.reduce((acc, row, idx) => {
    //正規表示式,只僅讓以下格式通過
    // 123
    // 123.45
    // 0.55
    // -123.45
    const valueText = row.inputValue.trim();
    //if (!/^\d*\.?\d{0,2}$/.test(val))  <-- 允許整數或小數點後最多2位
    const check_isnumber_Reg =  /^-?\d+(\.\d+)?$/.test(valueText);
   
    //如果輸入為空值 或 非數字格式
    if (!check_isnumber_Reg) {
      acc.error_Rows.push({
        index: idx + 1,
        stage: row.date_stage_code ,
        status: valueText === "" ? "空值":"非數值"
      });
      return acc;
    }
    
    const value = Number(row.inputValue.trim()) || 0;
    acc.count += 1;
    acc.sum += value;
    return acc;
  },
  {
    count: 0,
    sum: 0,
    error_Rows: []
  }
 );
 
  //先行確認unit type 
  useEffect(() => {

   adjust_unit_refix ? setAllocate_select_need(false) : setAllocate_select_need(true);

    if(!adjust_unit_refix){
      console.log( "一開始單位為重量系列判定:");
      setAll_FinalWeight(parseFloat(number_request_value).toFixed(2));
      setConfirm_LastUnit(unit_fields);
    } else{
      console.log( "執行為pcs或qty或roll系列判定:");

    } 
      
   }, [adjust_unit_refix]);

   useEffect(() => {
      if (
        prevWeightRef.current !== "" &&
        prevWeightRef.current !== allocate_baseweight
      ) {
        handle_backfirststep();
      }

      prevWeightRef.current = allocate_baseweight;
}, [allocate_baseweight]);


    // 監聽 Radio或日期 Button 切換
  const handle_Change = async (e) => {
    const { name, value } = e.target;

    if (name === "prorated_method") {
      setSelectedAllocateCase({ ...allocatecase, [name]: "prorated" });
	    setRadioMethod("prorated");
      setEnable_RadioMode(true);
      setallocate_DataRows([]);
    } else if (name === "manual_method") {
      setSelectedAllocateCase({ ...allocatecase, [name]: "mannul" });
      setRadioMethod("mannul");
      setEnable_RadioMode(true);
      setallocate_DataRows([]);
    } else if(name === "trip-start"){      
      setAllocstage_Calculate(value);       
    }else if(name === "avg-set"){      
      setfloat_support(value);       
    } 

    //清除既有的分配欄位及對應當前選的
   // setallocate_DataRows([]);
  };          


  //切換最小單位重量為
  useEffect(() => {
    console.log("是否為有效數字:",!isNaN(Number(allocate_baseweight)));  
    console.log(`選擇單位為:${allocate_baseweight} ${baseuiut_type}`);

    //1. 判定若是pcs,qty計價單位,則需要換算, 其他則就原始重量
    const total_weight = adjust_unit_refix ? allocate_baseweight * number_request_value : number_request_value;          
    const unitText_final = adjust_unit_refix ? String(baseuiut_type) : String(unit_fields);
  
    // 2. 轉換為數字前先確認 final_weight 存在
    const weightNum = parseFloat(total_weight).toFixed(2);
    const weightDisplay = isNaN(weightNum) ? "0.00" : weightNum;

    //3. 存入暫存後續導入分配
    setAll_FinalWeight(weightNum);
    setConfirm_LastUnit(unitText_final);
  
         
  }, [allocate_baseweight,baseuiut_type]);


  //切換日期算出實際週期
  useEffect(() => {
    
    console.log(`日期:${allocstage_calculate} 計算為第`+getWeekOfYear(allocstage_calculate) + "週");

    //針對手動部分做"週期"同步更新整個數列數據
    if( radiomethod  === "mannul"){
      const stage_d = String(getWeekOfYear(allocstage_calculate)).trim();

      const save_stage =Number(stage_d) < 10
                      ? stage_d.padStart(2,"0")
                      : stage_d;

      const shift_thursday = new Date(allocstage_calculate);

      shift_thursday.setDate(shift_thursday.getDate() +4 -(shift_thursday.getDay() || 7));

      const house_wave_year = shift_thursday.getFullYear();

      setallocate_DataRows(prev =>
        prev.map((item,index)=>{

          //原先結構為 物名編碼+廠商碼+年週期 ,因應年週期會有更新可能,這邊只取前2個做為永久不變動
          const parts = item.date_stage_code.split("-");

          // console.log("item=", item);
          // console.log("date_stage_code=", item.date_stage_code);
          // console.log("parts原先為=", parts);

          return {
            ...item,
            date_stage_code:
              `${parts[0]}-${parts[1]}-${parts[2]}-${parts[3]}-${house_wave_year}${save_stage}-${String(index+1).padStart(3,"0")}`
          };
        })
      );
    }
  }, [allocstage_calculate]);

  const handleCancel = () => {

     onHide();
  };


  const handle_NextStep = (e) => {
      e.preventDefault();
     //判定單位量不是為數值
     if(isNaN(Number(allocate_baseweight))){
        toast.error(`單位重量數值:${allocate_baseweight}是非數字格式!`);
        setAllocate_select_need(false)        
        return;     
     }

     //且當為小等於0
     if(!isNaN(Number(allocate_baseweight)) && Number(allocate_baseweight) <=0){
        toast.error(`輸入重量數值:${allocate_baseweight},目前低於或等於0,異常!`);
        setAllocate_select_need(false)
        return;  
     }

     //當確認無誤(數值(浮點數),且不為0的情況下)
     setAllocate_select_need(true);    
     
  };


  //最後計算分配的總重量(單位)
  useEffect(() => {
    
    console.log(`最後要分配總重量(單位)為: ${final_weight} / ${confirm_lastunit}`);
    
  }, [final_weight,confirm_lastunit]);

   //分配物料結構邏輯底下
    useEffect(() => {
      // 當顯示enable_radiomode
      if(enable_radiomode){
        const select_day = new Date(allocstage_calculate);
        const shift_thursday = new Date(select_day);    // 複製一份日期避免修改原物件
        shift_thursday.setDate(shift_thursday.getDate() + 4 - (shift_thursday.getDay() || 7));  // 找到該週星期四
        const erp_year = shift_thursday.getFullYear();

        //取得當前選擇週期
        const stage_d = String(getWeekOfYear(allocstage_calculate)).trim(); 
         

        //自動分配
        if(String(radiomethod)=== "prorated"){                      
            buildProratedRows( erp_year , stage_d ,allocatePCS);
        } //手動執行分配
        else{

        }
      } 
    }, [ enable_radiomode,allocstage_calculate,allocatePCS,float_support]);


  //回到初始第一步
  const handle_backfirststep = () => {
    setAllocate_select_need(false);
    setEnable_RadioMode(false);
    setRadioMethod("");
    setallocate_DataRows([]); //清空reset init
    setAllocatePCS(1);
  };

  const select_suitable_unit = ( reciver_unit) => {     
    const str_conv_unit = String(reciver_unit).trim().toLowerCase();
    console.log("接收單位為:"+ str_conv_unit);
    return mapping_unit_zthw_ch[str_conv_unit] || "未知";
  };

  const handleInputChange = (e, idx) => {
    const value = e.target.value;
    const num_content = Number(value);

    setallocate_DataRows(prev =>
      prev.map((item, index) =>
        index === idx
          ? {
              ...item,
              inputValue: value,
            }
          : item
      )
    );

    //若有不符合數值結構的內容這邊紀錄提醒操作者
    setInputErrors(prev => ({
      ...prev,
      [idx]:
        value === ""
          ? ""
          : isNaN(num_content)
          ? "只能輸入數字"
          : num_content < 0
          ? "不能小於0"
          : "",
    }));
  };


   //確認手動模式的輸入值狀態
   useEffect(() => {
      //檢驗手動模式下輸入的狀態
      if(radiomethod === "mannul" && allocate_dataRows.length > 0){
          // 有偵測到異常數量
          if (Mannul_Result.error_Rows.length > 0) {
            const msg = Mannul_Result.error_Rows
              .map(item => `第${item.index}筆(${item.stage})入倉碼keyin值異常, 狀態為:(${item.status})`)
              .join("、");                        
            console.log(`${msg} ,後續無法提交作業!`);            
          }   
          
          //偵測目前提交數值總和
          if(Mannul_Result.count > 0 &&  Mannul_Result.sum > 0){                
             console.log(`目前提交分配量總和為-> ${parseFloat(Mannul_Result.sum).toFixed(2)}`);

          }else{
            console.log(`目前無任何提交量數值:${Mannul_Result.sum} ! 數量為:${Mannul_Result.count}`);
          }

      }
    }, [allocate_dataRows]);


  const handleSubmit_allocate = async (e) => {

    e.preventDefault();  //預防未keyin就提交

     

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
                          style={{ marginLeft: "70px", backgroundColor: "red" , alignItems:"center" ,width:"70px" }}
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
                      .slice(1, 7)
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
              <div className="allocate-switch">                
                <input type="checkbox" id="switch"/>
                <label htmlFor="switch">
                    <span className="switch-txt"
                          turn_allocate="生產用料"
                          turn_direct="一般通用"
                     >    
                    </span>
                </label>
                <p style={{alignItems:"center", fontSize:"1.0rem" , fontStyle:"oblique"}}>模式|Mode</p>
              </div>
               {/* 當單位為pcs 以下需要做重量設定在往下一步*/}
               { adjust_unit_refix &&           
                  <div
                          className="mb-3"
                          style={{
                            textAlign: "center",
                            backgroundColor: "#FFDAC8",
                            padding: "20px 10px",
                          }}
                        >
                        <label htmlFor="allcate_code">
                          <strong style={{ fontSize: "26px" }}>請先設置{unit_fields}單位→</strong>
                        </label>
                      <input
                        // type={isPasswordVisible ? "text" : "allcate_code"}  
                        placeholder="單位輸入(小數2位)..."           
                        value={allocate_baseweight}
                        name="allcate_code"
                        className="shift-input allcate_code"
                        style={{
                          width: "20%",
                          paddingleft: "20px",
                          fontSize: "20px",
                          paddingRight: "10px", // 留出空間給按鈕
                          borderRadius: "4px",
                          border: weightError.includes("只能輸入數字")|| (Number(allocate_baseweight) ===0 || Math.sign(Number(allocate_baseweight)) === -1)
                                  ? "5px solid red"
                                  : "3px solid #a3d696",
                        }}
                        onChange={(e) => {               
                              const val = e.target.value;
                              
                              // 永遠更新畫面
                              setAllocate_BaseWeight(val);

                              // 允許空值
                              if (val === "") {
                                setAllocate_BaseWeight("");
                                setWeightError("");
                                return;
                              }

                              // 驗證是否為數字(含浮點)
                              // if (/^\d*\.?\d*$/.test(val)) {                        
                                // 驗證數字(含科學記號)
                              if (!isNaN(Number(val))) {
                                setWeightError("");                       
                              } else {
                                setWeightError("只能輸入數字");
                              }                 
                        }}                
                        onBlur={() => {
                          if ( allocate_baseweight !== "" && !isNaN(allocate_baseweight)) {
                            setAllocate_BaseWeight(
                              Number(allocate_baseweight).toFixed(2)
                            );
                          }
                        }}           
                      /> 
                      <select
                          onChange={(event) => setbaseuiut_type(event.target.value)}
                          style={{
                            width: "170px",
                            height: "50px",
                            marginRight:"10px",
                            fontSize: "1.6rem",
                            backgroundColor: "#cbf1bb",
                          }}
                        >
                          {displayUnitOptions.map((item, index) => (
                            <option key={item.type} value={item.val}>
                              {item.val}{"("}{item.type}{")"}
                            </option>
                          ))}
                        </select>  
                            
                        <Button variant="primary"  name="base_unit"  style={{paddingRight:"20px"}} onClick={(e) => handle_NextStep(e)}>
                          到下一步▼
                        </Button>
                        <span
                          style={{
                            fontSize: "19px",
                            marginBottom: 2,
                            color: "#0b044b",
                            borderBottom: "5px solid #555",                       
                            width: 190,
                            paddingLeft:"30px",
                            fontWeight:"bold"
                          }}
                        >
                          共計{select_suitable_unit(baseuiut_type)}為: {final_weight}{adjust_unit_refix?baseuiut_type:unit_fields}
                      </span>
                  </div>          
                }

                  {allocate_need && 
                    <div className="radio-container">
                    <label className="radio-item">
                      <input
                        type="radio"
                        name="prorated_method"
                        value={allocatecase.prorated_method}
                        checked={radiomethod === "prorated"}
                        onChange={handle_Change}
                      />
                      <p className="dbselect">平均分配</p>
                    </label>
                    <label className="radio-item">
                      <input
                        type="radio"
                        name="manual_method"
                        value={allocatecase.manual_method}
                        checked={radiomethod === "mannul"}
                        onChange={handle_Change}
                      />
                      <p className="dbselect">手動自行分配</p>
                    </label>
                  </div>                   
                }                         
        </div>                
        {/*當選擇好分配模式,顯示以下元件*/}
        { enable_radiomode  &&
            <div style={{ display: "flex", flexDirection: "column", alignItems: "center" , gap:"30px" }}>
                {/* 上方控制列 */}
                <div
                  style={{
                    display: "flex",
                    alignItems: "center"
                  }}
                >

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

                <button
                    onClick={handle_backfirststep}
                    className="nav-button"     
                    style={{ marginLeft: "30px" , display: adjust_unit_refix ? "inline-flex" : "none"}}   
                >
                  <span className="nav-icon">&#695;
                      回上一步←
                  </span>
                </button>                  
                <Button
                  className="button"                   
                  id="send_alloc"
                  style={{marginLeft: "30px" , border:"10px"}}
                  onClick={handleSubmit_allocate}
                >
                  提交分配
                </Button>
               </div>
                {/* prorated 平均自動分配*/}
                {radiomethod === "prorated" && (
                 <div
                       style={{
                          width: "80%",
                          border: "1px solid #ccc",
                          borderRadius: "8px",
                          padding: "15px",
                          background: "#f7f7f7"
                        }}
                   >
                  {/* 分配PCS */}
                  <div
                    style={{
                      marginBottom: "15px"
                    }}
                  >
                    <span style={{font:"caption", fontSize:"20px"}}>
                      分配包材量:
                    </span>
                    <input
                      type="number"
                      style={{
                        marginLeft: "10px",
                        width: "120px",
                        alignItems:"center",
                        color:"rgb(22, 22, 17)",
                        backgroundColor:"rgb(231, 227, 213)"
                      }}
                      value={allocatePCS}
                      placeholder="最多10000包"
                      min={1}
                      max={10000}
                      onChange={(e) =>
                        setAllocatePCS(Number(e.target.value))
                      }
                   />
                    <label style={{paddingLeft:"10px"}}>
                       精準浮點:
                      <select
                        name="avg-set"
                        style={{marginRight:"5px" ,  transform: "translateX(5px)"}}
                        value={float_support}
                        onChange={handle_Change}
                      >
                      <option value="0">否</option>
                      <option value="1">是</option>          
                      </select>
                    </label>
                   </div>
                   <div> 
                     <span
                        style={{fontSize:"2rem"}}
                        >入倉號編碼 </span>
                    </div>
                    <div  
                      style={{
                          maxHeight: "400px", // 最大高度
                          overflowY: "auto", // 超過出現捲軸
                          overflowX: "hidden",
                          paddingRight: "8px",
                        }}
                      >                   
                        {allocate_dataRows.map((row, idx) => (
                          <div 
                              style={{
                                display: "flex",
                                gap: "20px",
                                padding: "8px 0",
                                borderBottom: "1px solid #71cfc7",
                                justifyContent: "space-between", // 平均分配左右空間
                                alignItems: "center",
                                gap: "20px",
                                padding: "12px 16px",
                                marginBottom: "12px", // 每個 row 間距
                              }}
                              key={idx}
                          >                        
                            {/* 左側日期區塊 */}
                            <span
                              style={{
                                fontSize: "22px",
                                fontWeight: "bold",
                                padding: "6px 180px",
                                borderRadius: "8px",
                                background: "rgba(0,0,0,0.12)", // 遮罩感
                                backdropFilter: "blur(4px)", // 毛玻璃效果
                                WebkitBackdropFilter: "blur(4px)",
                                boxShadow: "0 2px 6px rgba(213, 216, 178, 0.15)",
                                color: "#020408",
                                minWidth: "220px",
                              }}
                            >
                              第{idx + 1}筆: {row.date_stage_code}
                            </span>
                            {/* 右側重量 */}
                              <span
                                style={{
                                  fontSize: "18px",
                                  fontWeight: 600,
                                  color: "#134e4a",
                                  flex: 1,
                                  textAlign: "right",
                                }}
                              >
                                {row.inputValue}
                                {" "}({g_unitText_type}/包)
                              </span>                       
                          </div>
                        ))}
                     </div>
                  </div>
                )}
                {/* mannul 手動分配 */}
                 {radiomethod === "mannul" && (  
                  <div
                    style={{
                      width: "80%",
                      border: "1px solid #ccc",
                      borderRadius: "8px",
                      padding: "15px",
                      background: "#f7f7f7"
                    }}
                  >
                  <div>                       
                      <span style={{font:"caption",fontSize:"2rem" ,marginBottom: "15px"}}>手輸入配置量 </span>
                      <button type="button" 
                                    className="btn btn-primary"
                                    onClick={() => addMannulCfg_value()}>
                            +
                       </button> 
                      </div>
		                  <div  
                        style={{
                            maxHeight: "400px", // 最大高度
                            overflowY: "auto", // 超過出現捲軸
                            overflowX: "hidden",
                            paddingRight: "8px",
                          }}
                        >                   
                        {allocate_dataRows.map((row, idx) => (
                          <div 
                              style={{
                                display: "flex",
                                gap: "20px",                           
                                borderBottom: "1px solid #71cfc7",
                                justifyContent: "space-between", // 平均分配左右空間
                                alignItems: "center",
                                gap: "20px",
                                padding: "5px 26px",
                                marginBottom: "3px", // 每個 row 間距
                                marginLeft: "auto"
                              }}
                              key={idx}
                          >                        
                            {/* 左側日期區塊 */}
                            <span
                              style={{
                                fontSize: "22px",
                                fontWeight: "bold",
                                padding: "2px 8px",
                                borderRadius: "8px",
                                background: "rgba(0,0,0,0.12)", // 遮罩感
                                backdropFilter: "blur(4px)", // 毛玻璃效果
                                WebkitBackdropFilter: "blur(4px)",
                                boxShadow: "0 2px 6px rgba(213, 216, 178, 0.15)",
                                color: "#020408",
                                minWidth: "300px",
                              }}
                            >
                              第{idx + 1}筆: {row.date_stage_code}
                            </span>							
                            {/* 右側量值 */}
                              <span
                                style={{
                                  fontSize: "10px",
                                  fontWeight: 101,
                                  color: "#134e4a",
                                  flex: 1,
                                  textAlign: "right",
                                  marginLeft:"auto"
                                }}
                              >
                                  <div
                                    style={{
                                      flex: 1,
                                      display: "flex",
                                      justifyContent: "flex-end",
                                      alignItems: "flex-start",
                                    }}
                                  >
                                    <div
                                      style={{
                                        display: "flex",
                                        flexDirection: "column",
                                        alignItems: "flex-end",
                                      }}
                                    > 
                                      <input
                                          type="text"
                                          className="form-control "            
                                          // style={{ width: "100px"  ,margintop:"5px" ,fontSize:"0.5rem" }}
                                          placeholder="輸入數字(小數2位)"                                  
                                          value={row.inputValue || ""}
                                          onChange={e => handleInputChange(e, idx)}
                                            style={{
                                                 width: "235px",
                                                  marginTop: "5px",
                                                  fontSize: "1.5rem",
                                                  paddingLeft: "8px",
                                                  paddingRight: "8px",
                                                  borderRadius: "6px",                                                  
                                                  border: inputErrors[idx]
                                                    ? "5px solid #EA0000"
                                                    : "3px solid #a3d696",
                                            }}                                                                   
                                      /> 
                                        {/*將錯誤訊息外框一併顯示*/}
                                        {
                                          inputErrors[idx] && (
                                                  <div
                                                    style={{
                                                      color: "red",
                                                      marginLeft: "10px",
                                                      fontSize: "14px",
                                                      marginTop: "5px"
                                                    }}
                                                  >
                                                    {inputErrors[idx]}
                                                  </div>
                                          )
                                        }						                               
                                    </div>
                                    <span
                                      style={{
                                        marginLeft: "85px",
                                        alignSelf: "center",
                                        fontSize:"1.5rem",
                                        fontStyle:"initial",
                                        color:"#006030"
                                      }}
                                    >
                                      ({g_unitText_type}/包)
                                    </span>
                                  </div>
                                </span>                                              
                              {/* Remove */}
                              <button
                                  type="button"
                                  className="btn btn-outline-danger"
                                  onClick={() => removeMannulCfg(idx)}
                                >
                                  −
						                </button>	
                            
                          </div>                          
                        ))}                       
                     </div>
                 </div>
               )
              }
            </div>
         }           
      </Modal>
  );
}

export default AllocationPopup_Work;