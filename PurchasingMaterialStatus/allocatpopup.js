import React, { useState, useEffect, useRef, Suspense ,useMemo , useCallback ,startTransition } from "react";
import { json, Route } from "react-router-dom";
import { Modal, Button, Card, Row, Col, Table, FormControl, Toast } from 'react-bootstrap';
import Form from "react-bootstrap/Form";
import axios from "axios";
import moment from "moment";
import 'moment/locale/zh-tw'; 
import { isArray, kebabCase, lowerCase } from "lodash";
import DatePicker from "react-datepicker";
import Skeleton from "react-loading-skeleton";
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
import { useAuth } from "../../context/GlobalProvider"; //引入Auth 權限身分者
// 導入 MessagePopup 組件
import MessagePopup from '../../components/MessagePopup';
import { faL } from "@fortawesome/free-solid-svg-icons";
import {
  FaFilePdf,
  FaFileWord,
  FaFileExcel,
  FaFileImage,
  FaPaperclip
} from "react-icons/fa";
import {
  Select,
  MenuItem,
  Checkbox,
  ListItemText,
} from "@mui/material";
const Confirm_AllocationModal = React.lazy(() => import("../../components/Confirm_AllocationModal")); // 確認按鈕加載組件


const allocate_info_key = [ "採購單號","工作序" ,  "物料名" , "規格", "編碼","數量","單位","廠商碼"];

//預設庫別
const warehouse_type = [ "TR-07","樹林物料總倉" , "觀音物料總倉" , "外租用總倉"];

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10mb
const ONE_DAY_MILSEC = 24 * 3600 * 1000   // 1個小時總毫秒數量

const lower_percentage_torlence = parseFloat("0.99"); //相當於可接受1%下限
const upper_percentage_torlence = parseFloat("1.01"); //相當於可接受1%上限

  //目前許可上傳的檔案格式
  const allowedExtensions = [
    "png",
    "jpg",
    "jpeg",
    "bmp",
    "pdf",
    "tiff"
    // "doc",
    // "docx",
    // "xlsx",
    // "xls",
  ]; // 允許的檔案副檔名

  // const allowed = [".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp"]; //目前許可上傳的檔案格式


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

const Issue_all_Options = [ "缺料" ,"外觀毀損瑕疵","檢驗特性不良"];

//初始化空數據組
  const createMannulValue ={
      date_stage_code: '',  //分配包裝辨識週期字串
      inputValue: '',       // 單位量值   
  };


const not_weight_units = [ "pcs", "qty" , "roll"]

// 正規表達式判斷是否包含英文和數字
const regex_repairerr = /^(?=.*[a-zA-Z])(?=.*\d).+$/;

function AllocationPopup_Work({ show, onHide,allocat_data }) {
  const { user } = useAuth();
  const [name, setName] = useState("");	
	const [memberID, setMemberID] = useState("");
  const [positionarea, setPositionarea] = useState([]); // 預設無部門
  const [storeitems, setStoreItems] = useState([]);
  const [locationitems, setLocationItems] = useState([]);
  const [allocate_baseweight, setAllocate_BaseWeight] = useState(0);   //判定units 若是PCS計量需要自訂義單位重量,預設為0公克
  const [final_weight, setAll_FinalWeight] = useState(0);
  const [confirm_lastunit, setConfirm_LastUnit] = useState("");  
  const [enable_radiomode, setEnable_RadioMode] = useState(false);
  const [allocate_need, setAllocate_select_need] = useState(false);
  const [modalIsOpen, setmodalIsOpen] = useState(false);
  const [baseuiut_type, setbaseuiut_type] = useState(unit_options[0]["val"]);   
  const [weightError, setWeightError] = useState("");
  const [inputErrors, setInputErrors] = useState({});  // 手動分配數值異常 (error紀錄顯示訊息提醒)
  const [allocate_dataRows, setallocate_DataRows] = useState([ { ...createMannulValue } ]); // 儲存多筆物料重量資料
  const [allocatecase, setSelectedAllocateCase] = useState({
    prorated_method: "",
    manual_method: "",
    normal_method: ""
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
  const [warehouse_label, setWarehouse_label] = useState("");   //庫別設定
  const [stackposition_label, setStackPosition_label] = useState("");  //倉位設定
  const prevWeightRef = useRef(allocate_baseweight); // 監聽狀態儲存(初始化)
  const [stack_codevalid, setStackCodeValid] = useState(false);  //倉位字串(英文加數字)是否有效
  const [allocaTarget, setAllocaTarget] = useState(null); // 儲存要分配的資料內容
  const [loading, setLoading] = useState(false); //增加提交緩衝判斷狀態
  const [uploadProgress,setUploadProgress] = useState(0);  //增加處理百分比進度
  const [istaskallocate_mode, setTaskAllocate_Mode] = useState(true); //預設一開始為"生產配料介面""
  // MessagePopup 狀態管理
  const [messagePopup, setMessagePopup] = useState({
      show: false,
      type: 'info',
      title: '',
      message: ''
  });

  const [check_iqc_mode, setCheckIQC_MODE] = useState(""); // 好 或 不良
  const [error_status, setError_status] = useState([]); // 多選錯誤情形
  const [isIssuecheck, setIsIssueChecked] = useState(false); //是否有物料不良或缺少
  const [ismustattached, setMustAttached] = useState(false);  
  const [isFilesUpload, setIsFilesUpload] = useState(false);
  const [isOverLimit, setOverLimit] = useState(false); // 初始化表單數據為空
  const [totalMB, setTotalMB] = useState(0); // 用於顯示上傳的檔案總大小
  const [file, setFile] = useState([]);
  const fileInputRef = useRef(null);

  
  // const key_prefix_purchstr = String(Object.values(allocat_data)[0]).slice(5);
  // console.log("接收allocat_data 資料型態為: "+ typeof allocat_data +  "前綴單號字串為:" + String(key_prefix_purchstr));

  !Array.isArray(allocat_data)?console.log("接收allocat_data 資料內容為: "+ JSON.stringify(allocat_data,null,2))
                              :console.log("接收allocat_data 資料內容List為: "+ Object.values( allocat_data));

  const purch_formId = Object.values(allocat_data)[0];
  const pur_pk_serialID = Object.values(allocat_data)[1];
  const pdc_name = Object.values(allocat_data)[2];
  const pdc_spec = Object.values(allocat_data)[3];
  const unit_fields = Object.values(allocat_data)[Object.values(allocat_data).length-2];  
  const number_request_value  = Object.values(allocat_data)[Object.values(allocat_data).length-3];      
  const item_encode =   Object.values(allocat_data)[Object.values(allocat_data).length-4];
  const venderid = Object.values(allocat_data)[Object.values(allocat_data).length-1];
  const adjust_unit_refix = not_weight_units.includes(lowerCase(unit_fields)) ;
  const [allocatePCS, setAllocatePCS] = useState(1 ||Math.ceil(Number(number_request_value)));   //預設為 allocate 分配為採購給予的pcs數值
  const [check_error_pcs, setCheck_Error_PCS] = useState(0);  //NG檢驗數量(預設:0)
  const [mux_add_subtrac, setTotalmux_Add_Subtrac] = useState(0);  //預設一般通用分配總數(預設:0),實際需要跟採購單進貨單總數一致才合理(若有NG也需要總和等同)

  const g_unitText_type = adjust_unit_refix ? String(baseuiut_type) : String(unit_fields);

  // console.log("目前年是: "+ nowyear);
  // console.log("今年第一天 是禮拜 "+ dayOfFirstDate);
  console.log("是否要重新定義物料重量:"+ adjust_unit_refix);
          
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

  function toMySQLDateTime(date) {
        const pad = n => String(n).padStart(2, "0");
        return (
            date.getFullYear() + "-" +
            pad(date.getMonth() + 1) + "-" +
            pad(date.getDate()) + " " +
            pad(date.getHours()) + ":" +
            pad(date.getMinutes()) + ":" +
            pad(date.getSeconds())
        );
  };
  
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

 const toggleGroup = (name) => {
  setError_status((prev) =>
    prev.includes(name)
      ? prev.filter((x) => x !== name)
      : [...prev, name]
  );
};

  const handleError_list_SelectChange = (event) => {
    const { value } = event.target;
    
    // if (isIssuecheck) return;

    //將選取的select item 依序填入arraylist
    setError_status(typeof value === "string" ? value.split(",") : value);   
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
      //const serial_houwave_code = final_itemconde_entry+'-'+ String(venderid) +'-' + house_wave_year+save_stage+'-'+ String(i + 1).padStart(3, '0');
      const serial_houwave_code = final_itemconde_entry+'-' + house_wave_year+save_stage+'-'+ String(i + 1).padStart(3, '0');

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
   

   //const serial_houwave_code =final_itemconde_entry +"-" + String(venderid) + '-' +house_wave_year +save_stage +"-" +String(allocate_dataRows.length + 1).padStart(3, "0");
   const serial_houwave_code =final_itemconde_entry +"-" +house_wave_year +save_stage +"-" +String(allocate_dataRows.length + 1).padStart(3, "0");


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

 //一般通用分配
 const buildNormalRows = async( house_wave_year , stage , avg_quantity , qcng_pcs) => {

  const temp_add_subtrac =  Math.ceil(avg_quantity + qcng_pcs);
  //存取分配加總後續提交判斷是否合理
  setTotalmux_Add_Subtrac(Number(temp_add_subtrac));

	//const avg_normal =  Number(avg_quantity - qcng_pcs);
  const avg_normal =  Number(avg_quantity );
	console.log("實際要存入倉內數量 = "+ parseFloat(avg_normal) + "NG和PASS加總為:" +  temp_add_subtrac  + "  IQC NG 數量為 = "+ parseFloat(qcng_pcs));

    const avg_row = [];
    const final_itemconde_entry = item_encode!==null ? item_encode.trim() :"??-???-???";

    for (let i = 0; i < avg_normal; i++) {
      //目前流水號從001~999 (最多一千筆)
      const save_stage = parseInt(stage) < 10 ? String(stage).padStart(2, '0'):String(stage);
     // const serial_houwave_code = final_itemconde_entry+'-'+ String(venderid) +'-' + house_wave_year+save_stage+'-'+ String(i + 1).padStart(3, '0');
      const serial_houwave_code = final_itemconde_entry+'-'+ house_wave_year+save_stage+'-'+ String(i + 1).padStart(3, '0');

      avg_row.push({
        date_stage_code: serial_houwave_code,
        inputValue:  ""   //預設每筆都"
      });
    }

   
    Number(qcng_pcs) > 0 ? setMustAttached(true):setMustAttached(false);  // 判定是否要提供附加文件啟動    
    setallocate_DataRows(avg_row); //存入自動分配紀錄

    
 }

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

  useEffect(() => {
    const fetchStore_Infolist = async () => {
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/purchsaleinvtory/store_nowList`
        //   "http://localhost:3009/purchsaleinvtory/store_nowList"
        );
        
        // console.log("目前倉庫別回傳為:"+ JSON.stringify(response.data.store_allname,null,2));
        // console.log("目前位置回傳為:"+ JSON.stringify(response.data.location_allname,null,2));
        
         setStoreItems(response.data.store_allname);
         setLocationItems(response.data.location_allname);

         
         setWarehouse_label(response.data.store_allname[0].name);
         setStackPosition_label(response.data.location_allname[0].name);
      } catch (error) {
        console.error("取得倉庫資訊列表錯誤", error);
      }
    };

    fetchStore_Infolist();
  }, []);

 useEffect(() => {
    try {
      if(user){
        startTransition(() => {
          console.log("user總組態:", user);
          // console.log("authPosition組態為:" + typeof user?.authPosition)
          // console.log("authPosition 取值為:" + Object.values(user?.authPosition));
          const auth_departpart = Object.values(user?.authPosition);
          setName(user?.reg_schedulename || "");
          setMemberID(user?.memberID || "無工號");
          setPositionarea(typeof auth_departpart === "string" ? String(user?.authPosition).replace(/,/g, " , ") : auth_departpart); 
          //  setPositionarea(String(user.positionarea).replace(/,/g, " , "));
        });
      }
    } catch (err) {
        console.error("AllocationPopup_Work get Auth Behavior error:", err);
    }

  }, [user]);
 
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

   //切換工作模式
   useEffect(() => {
      if (istaskallocate_mode) {
      // 生產配料
      setSelectedAllocateCase({});      
      setEnable_RadioMode(true);
      setallocate_DataRows([]);
      setCheck_Error_PCS(0);
      setFile([]);
    } else {
      //一般通用
      console.log("切換到一般通用入庫模式"); 
      setEnable_RadioMode(true);    
      setallocate_DataRows([]);
      setCheck_Error_PCS(0);
      setOverLimit(false)
      setFile([]);
    }

  }, [istaskallocate_mode]);

   useEffect(() => {
      if (
        prevWeightRef.current !== "" &&
        prevWeightRef.current !== allocate_baseweight
      ) {
        handle_backfirststep();
      }

      prevWeightRef.current = allocate_baseweight;
}, [allocate_baseweight]);

  const handleCheckIssueChange = (event) => {
    const checked = event.target.checked;
    setCheckIQC_MODE(check_iqc_mode === "Error" ? "" : "Error");
    //增加手動異常判斷下列邏輯
    setIsIssueChecked(checked);    
    if (checked) {
      // const allissue = Issue_all_Options.map((opt) => opt).join(", ");
      // console.log("所有問題列-> " + allissue);
      // setError_status(allissue);      
    } else {
      setError_status([]);
    }
  };


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
    }else if (name === "normal_method") {
      setSelectedAllocateCase({ ...allocatecase, [name]: "normal" });
      setRadioMethod("normal");
      setEnable_RadioMode(true);
      setallocate_DataRows([]);
    }      
    else if(name === "trip-start"){      
      setAllocstage_Calculate(value);       
    }else if(name === "avg-set"){      
      setfloat_support(value);       
    }else if(name === "warehouse"){      
      setWarehouse_label(value);       
    } else if(name === "stack_position"){      
      setStackPosition_label(value);    
      
       //取倉位字串當下輸入狀態有錯誤以下可能
        if (!regex_repairerr.test(value)) {
          setStackCodeValid(false);
        } else {
          setStackCodeValid(true);
        }
    }else if( name === "pass_check"){
        setCheckIQC_MODE(check_iqc_mode === "Pass" ? "" : "Pass");
        setError_status([]);
        setCheck_Error_PCS(0);
        setIsFilesUpload(false);
        setIsIssueChecked(false);
    }

    //清除既有的分配欄位及對應當前選的
   // setallocate_DataRows([]);
  };          

  const handleFileChange = async (event) => {     
    const files = Array.from(event.target.files);
    setOverLimit(false);
    setIsFilesUpload(false);
    let totalSize = 0 , check_allow_filecount = 0;

    if (!files || files.length === 0 || !files.length) {
      setFile([]);
      setTotalMB("0.00");
      setOverLimit(false);   // 初始化為未超過限制
      setIsFilesUpload(false); // 初始化為未上傳開啟功能
      if (fileInputRef.current) {
          fileInputRef.current.value = "";
      }
      return;
   }


    if (event && event.target && event.target.files) {
      const selectedFiles = Array.from(event.target.files);
      // const now = moment().format("YYYYMMDD");

      const title = `${item_encode}-${venderid}-NG`;

      let renamedFiles = [];
      //目前需要將選擇的檔案名稱 rename
      //格式: moment(當前日期)-title-流水號-檔案名稱.副檔名
      selectedFiles.forEach((file, idx) => {
        // console.log(`第 ${idx} 項： 檔案:${file}`);
        // // 或 push 到另一個陣列 newArr.push(item)
        const ext = file.name.slice(file.name.lastIndexOf("."));
        const single_size = file.size;
        const newdefineName = `${title}-${idx+1}`; // 格式: 日期-物料碼-廠商號-序號.ext

        const fileExtension = file.name.split(".").pop().toLowerCase();

        if (!allowedExtensions.includes(fileExtension)) {
          // 如果檔案副檔名不在允許的清單中，拒絕上傳
          console.log("拒絕上傳：", fileExtension);
        } else {
          console.log("允許上傳：", file.name);
          renamedFiles.push(file);
        }

      //  renamedFiles.push(new File([file], newdefineName, { type: ext }));
      
        totalSize += single_size; // 累加檔案大小

        if (totalSize > MAX_FILE_SIZE) {
          setOverLimit(true);
        }
      
      });

      const cal_totalMB = totalSize / (1024 * 1024);
      setTotalMB(cal_totalMB.toFixed(2)); // 更新總大小狀態

      //初始化errorfile
      if(Number(renamedFiles.length) > 0 ){
        console.log("目前提交上傳檔案數量為:"+ Number(renamedFiles.length));
        setIsFilesUpload(true);
      }      
      setFile(renamedFiles);
    }
  };

   const InitPrevent_allset = () => {
    setFile([]);
    
    if (fileInputRef.current) {
        fileInputRef.current.value = "";
    }  
  }

  //切換最小單位重量為
  useEffect(() => {
    console.log("是否為有效數字:",!isNaN(Number(allocate_baseweight)));  
    console.log(`選擇單位為:${allocate_baseweight} ${baseuiut_type}`);

    //1. 判定若是pcs,qty計價單位,則需要換算, 其他則就原始重量

    //若有NG不良的狀況,需要參照
    const Ref_isNG_Result_Number = isIssuecheck && Number(check_error_pcs) >0 ? Number(number_request_value-check_error_pcs):number_request_value;
    const total_weight = adjust_unit_refix ? allocate_baseweight * Ref_isNG_Result_Number : Ref_isNG_Result_Number;          
    const unitText_final = adjust_unit_refix ? String(baseuiut_type) : String(unit_fields);
  
    // 2. 轉換為數字前先確認 final_weight 存在
    const weightNum = parseFloat(total_weight).toFixed(2);
    const weightDisplay = isNaN(weightNum) ? "0.00" : weightNum;

    //3. 存入暫存後續導入分配
    if(String(radiomethod)=== "normal"){       
      setAll_FinalWeight(Number(allocatePCS));
      setConfirm_LastUnit(unit_fields);

    }else{
      setAll_FinalWeight(weightNum);
      setConfirm_LastUnit(unitText_final);
    }

  }, [allocate_baseweight,baseuiut_type,isIssuecheck,check_error_pcs,radiomethod]);


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
              //`${parts[0]}-${parts[1]}-${parts[2]}-${parts[3]}-${house_wave_year}${save_stage}-${String(index+1).padStart(3,"0")}`
              `${parts[0]}-${parts[1]}-${parts[2]}-${house_wave_year}${save_stage}-${String(index+1).padStart(3,"0")}`
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

     //當若有參照NG數量加總後,配置小於等於0(負值)則不通行
     if(Number(final_weight) <= 0){
        toast.error(`配置總統計為:${final_weight},目前為0或小於0,無法配置!`);
        setAllocate_select_need(false)        
        return;      
     }

     //若選擇有異常狀態,但值為0則不通行
     if( check_iqc_mode === "Error" && Number(check_error_pcs) === 0){
        toast.error(`目前已切換異常模式,數量為:${check_error_pcs},無法配置!`);
        setAllocate_select_need(false)        
        return;      
     }

     //當確認無誤(數值(浮點數),且不為0的情況下)
     setAllocate_select_need(true);    
    //  console.log("實際不良原因為:"+ error_status);
     
  };

  const getFileIcon = (fileName) => {
    const ext = fileName.split(".").pop().toLowerCase();
    if (ext === "pdf") return <FaFilePdf style={{ color: "red" }} />;
    if (ext === "doc" || ext === "docx")
      return <FaFileWord style={{ color: "blue" }} />;
    if (ext.includes("xls") || ext === "csv") return <FaFileExcel style={{ color: "green" }} />;
    if (ext === "sql") return <FaPaperclip style={{ color: "grey" }} />;
    if (     
      ext !== "pdf" 
    ) {
      return <FaFileImage style={{ color: "orange" }} />;
    }
    return null;
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
            buildProratedRows( erp_year , stage_d ,allocatePCS );
        }
        // 一般通用分配
        else if(String(radiomethod)=== "normal" ){                      
            buildNormalRows( erp_year , stage_d ,allocatePCS , check_error_pcs );
        }          
        //手動執行分配
        else{
        }

        //判定是否要開啟異常附加檔案提交元件(非一般模式下)
        if(String(radiomethod) !== "normal"){
          Number(check_error_pcs) > 0 ? setMustAttached(true):setMustAttached(false);  // 判定是否要提供附加文件啟動
        }

      } 
    }, [ enable_radiomode,allocstage_calculate,allocatePCS,check_error_pcs,radiomethod, float_support]);

  //回到初始第一步
  const handle_backfirststep = () => {
    setAllocate_select_need(false);
    setEnable_RadioMode(false);
    setRadioMethod("");
    setallocate_DataRows([]); //清空reset init
    setAllocatePCS(1);
    setFile([]);
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
          
          const input_empty_count =  Math.ceil( Number(allocate_dataRows.length) - Number(Mannul_Result.count)).toString();

          //偵測目前提交數值總和
          if(Mannul_Result.count > 0 &&  Mannul_Result.sum > 0){                


             console.log(`目前提交分配量總和為-> ${parseFloat(Mannul_Result.sum).toFixed(2)}  \r\n 數量為:${Mannul_Result.count} \r\n 尚未輸入值的數量為:${input_empty_count}`);
             
          }else{
            console.log(`目前無任何提交量數值:${Mannul_Result.sum} ! 數量為:${Mannul_Result.count}`);
          }

      }
    }, [allocate_dataRows]);

  
   const handle_Allocation_OnHide =  async (meg) => {
    setmodalIsOpen(false);

    if(meg ==="Yes")
    {      
      //執行分配指定採購form_id指向item並綁定各入倉週期編碼
       Implement_Erp_MaterialAllcation(allocaTarget);
      // toast.success("分配料成功!");  
    }else{
      // toast.success("取消分配物料作業!");  
    }    
  };


  const handleSubmit_allocate = async (e) => {
    e.preventDefault();  //預防未keyin就提交

    let final_sum_calculation = parseFloat(Mannul_Result.sum).toFixed(2);
    const input_empty_count =  Math.ceil( Number(allocate_dataRows.length) - Number(Mannul_Result.count)).toString();

    const lower_allowable_value = Number((final_weight * lower_percentage_torlence).toFixed(2));
    const upper_allowable_value = Number((final_weight * upper_percentage_torlence).toFixed(2));

    const diff_tolence_val = Number(final_sum_calculation - final_weight).toFixed(3);
    const all_allocate_packet = Number(Mannul_Result.count);
    
    console.log(`單號採購量為:${Number(final_weight)} \r\n` +"最後要提交的總量為:"+final_sum_calculation +  '\r\n'+"配發量為:" + Mannul_Result.count +  '\r\n'+"與實際採購誤差量:"+ diff_tolence_val);
    
    console.log("radiomethod 模式目前為:"+radiomethod);

    //以下為確認提交資訊(有輸入計總量低於5%下限,或空值或不合法的狀態),此機制只針對手動模式判斷
    if( radiomethod  === "mannul" ){           
      if(input_empty_count > 0){
        showMessage('error','確認提交資訊(有輸入空值或不合法的狀態),請確認!');
        return; 
      }else if(allocate_dataRows.length === 0){
        showMessage('warning','請增加配置量!');
        return;   
      }else if (final_sum_calculation < lower_allowable_value ){
        showMessage('error',`目前的提交總量:${final_sum_calculation},\r\n已低於下限量(lowlimit)->${lower_allowable_value}\r\n 請輸入符合之總量值!`);
        return;
      } else if ( final_sum_calculation > upper_allowable_value){
        showMessage('error',`目前的提交總量:${final_sum_calculation},\r\n已高於上限量(uplimit)->${upper_allowable_value}\r\n 請輸入符合之總量值!`);
        return;
      }        
    }   
    else{
        //自動分配模式
      if( radiomethod  === "prorated" )
      {
           if(allocate_dataRows.length === 0 || Number(final_sum_calculation) <= 0){
              // console.log("自動分配模式目前輸入資訊量為:"+ Number(allocate_dataRows.length));
              showMessage('warning','請選擇分配包材量!'); 
              return;
           }
      }
      // 一般入倉通用模式
      else
      {
          const all_normal_packet = Math.ceil(Number(mux_add_subtrac));  //實際要提交之總量
          const form_order_quantity =  Math.ceil(Number(number_request_value)); //採購單數量

          //只針對存取數據量不能空 
           if(allocate_dataRows.length === 0){
              showMessage('warning','請選擇分配包材量!'); 
              return;
           }

           //有檢驗NG數量,若無提供附加文件(圖檔或pdf)則卡控
          if( Number(check_error_pcs) > 0 && file.length === 0 ){
              showMessage('error','請至少提供一個至多個異常附加文件(圖片,pdf)!'); 
              return;
          }

          //  if(!isFilesUpload){
          //     showMessage('error','沒有提供上傳異常附加檔案!'); 
          //     return;
          // }
           //超出附加檔案總大小限制
          if(isOverLimit){
              showMessage('error','已經超出附加檔案總大小限制(10MB)!'); 
              return;
          }
                   
          //針對IQ檢驗OK數量和NG數量加總判斷, 與採購單號數量需要一致
          if ( all_normal_packet < form_order_quantity  ){
            showMessage('error',`目前的進料入倉總量:${all_normal_packet},\r\n已低於採購單量(lowlimit)->${form_order_quantity}\r\n 請輸入符合之總量值!`);
            return;
          } else if (all_normal_packet > form_order_quantity ){
            showMessage('error',`目前的進料入倉總量:${all_normal_packet},\r\n已高於採購單量(uplimit)->${form_order_quantity}\r\n 請輸入符合之總量值!`);
            return;
          }

          // console.log(`OK ->目前的進料入倉總量:${all_normal_packet},已平於於採購單量(uplimit)->${form_order_quantity}\r\n 符合之總量值.準備提交prepare!`);
          // return;
      }    
    }

    //這邊順勢檢查倉位是否合理字串
    // if(!stack_codevalid){          
    //   toast.error("倉位字串有錯誤(應為:英文加數字)!");
    //   return;
    // }

    // console.log("不良原因狀態為:"+error_status +  "error_status 結構為 " + typeof error_status  + "  check_iqc_mode 模式為:"+ check_iqc_mode + "  isIssuecheck 布林狀態為:"+ isIssuecheck )


    //有檢驗NG數量,若無提供附加文件(圖檔或pdf)則卡控
    if( Number(check_error_pcs) > 0 && file.length === 0 ){
        showMessage('error','請至少提供一個至多個異常附加文件(圖片,pdf)!'); 
        return;
    }

    // console.log("radiomethod模式為=" + radiomethod+ " - 不良原因狀態為:"+error_status +  "error_status 結構為 " + typeof error_status  + "  check_iqc_mode 模式為:"+ check_iqc_mode + "  isIssuecheck 布林狀態為:"+ isIssuecheck )


    if( check_iqc_mode === "Error" && isIssuecheck ){  
          
          if( error_status.length === 0 || error_status.includes("")){
            showMessage('warning','沒有選擇異常原因,請選擇至少一項!'); 
            return;
          }

          if(!isFilesUpload){
              showMessage('error','沒有提供上傳異常附加檔案!'); 
              return;
          }

           //超出附加檔案總大小限制
          if(isOverLimit){
              showMessage('error','已經超出附加檔案總大小限制(10MB)!'); 
              return;
          }
      }

    // 權限通過 → 打開二次確認 Modal
    setAllocaTarget({ final_weight, final_sum_calculation, all_allocate_packet , diff_tolence_val, g_unitText_type , name,memberID ,radiomethod});
    setmodalIsOpen(true);
  };

  //將要提交的組態重新打包
  const prepare_submit_allocateData = ( sum_calcula_number ,task_name, task_memid ) => {

    let payload =[];
    const submitTime = new Date(); // JS Date 物件

    //將 submitTime 轉 MySQL DATETIME 格式
    const create_date = toMySQLDateTime(submitTime);
  
    // console.log("原先要分配組態內容為:"+ JSON.stringify(allocate_dataRows,null,2));

    const auth_convert_type  =!Array.isArray(positionarea) ? JSON.parse(positionarea).join('_'):positionarea.join('_');

    //確認檔案狀態
    const issue_filelist = file.length > 0
                          ? file
                              .map(f => f.name)
                              .filter(name => name !== "")
                              .join(",")
                          : "";

    //確認有內容,至少一組
    if(allocate_dataRows.length > 0 ){          
        //  allocate_dataRows.map(( row , index)=>{                         
        //     const temp = [ 
        //       create_date ,              // 提交日期(時間) 
        //       purch_formId,              // 採購單號
        //       pur_pk_serialID,           // 採購單table pk 序號
        //       product_name ,             // 物料名稱
        //       specification,             // 規格
        //       row.date_stage_code,       // 入倉編碼
        //       sum_calcula_number,        // 計算totalb入總量              
        //       row.inputValue,            // 每批次量
        //       g_unitText_type,           // 計算單位
        //       task_name,                 //分配者姓名
        //       task_memid,                //分配者工號
        //       positionarea,              //分配者所屬部門
        //       warehouse_label,           //庫別
        //       stackposition_label        //倉位
        //     ];
        //     payload.push(temp)
        // });

        return allocate_dataRows.map(row => [
            create_date,
            purch_formId,
            pur_pk_serialID,
            String(item_encode)||"?",
            String(pdc_name)||"",
            String(pdc_spec)||"",
            String(venderid)||"",
            row.date_stage_code,
            String(radiomethod)=== "normal"? Number(final_weight+check_error_pcs):sum_calcula_number,
            !isNaN(parseFloat(row.inputValue))? parseFloat(row.inputValue).toFixed(2): "",
            String(radiomethod)=== "normal"? unit_fields:g_unitText_type,
            Number(check_error_pcs),
            unit_fields,
            String(error_status).trim("")||"",
            task_name,
            task_memid,
            auth_convert_type,
            warehouse_label,
            stackposition_label,
            String(radiomethod).trim(""),
            issue_filelist         
        ]);
    }

    return payload;

  };


  //執行分配api運行
  const Implement_Erp_MaterialAllcation = async ( allocaTarget) => {
       const {  final_weight, final_sum_calculation, all_allocate_packet , diff_tolence_val, g_unitText_type , name,memberID } = allocaTarget || {};
       
       const formData_final = prepare_submit_allocateData( final_sum_calculation,name,memberID);

      //確認檔案狀態
      const issue_filelist = file.length > 0
                            ? file
                                .map(f => f.name)
                                .filter(name => name !== "")
                                .join(",")
                            : "";
     
       const final_allocate_packet = (radiomethod === "mannul") ? parseInt(Mannul_Result.count):parseInt(allocatePCS);
       
      //  const convert_json_formData = JSON.stringify(formDataToSend);
        
      //  !Array.isArray(formDataToSend)?console.log("即將要分配組態內容為 formDataToSend: "+ JSON.stringify(formDataToSend,null,2))
      //                         :console.log("formDataToSend 資料內容List為: "+ Object.values(formDataToSend)+ "一共"+Object.values(formDataToSend).length +"筆資料");
       
      // formDataToSend.forEach((item, index) => {
      //   console.log(`第 ${index + 1} 筆資料`);
      //   console.log(JSON.stringify(item, null, 2));
      // });   
      
      //預防重複click響應
      if (loading) return;

      setLoading(true);

      // 前端 HTTP 分批大小
      const API_BATCH_SIZE = 100;
       
      try {      
            const totalCount = formData_final.length;
            let successCount = 0;
            for (let i = 0 ; i < totalCount; i += API_BATCH_SIZE) {
                
                  const chunk_bat = formData_final.slice(
                    i,
                    i + API_BATCH_SIZE
                  );

                  const formDataToSend = new FormData();

                  //---start-------------
                  //資料跟檔案要一起送
                  formDataToSend.append(
                      "allocateData",
                      JSON.stringify(chunk_bat)
                  );

                   //檔案批送
                  if (issue_filelist!=="") {
                      file.forEach((fname, idx) => {
                        formDataToSend.append("files", fname);          
                      });
                  }

                  //紀錄批次序號當(chunkIndex === 0)後端判斷完整寫入空間和存庫,其餘都只存庫
                   formDataToSend.append("chunkIndex", i / API_BATCH_SIZE ); 
                  //---end-------------

                  const res = await axios.post(
                    `${config.apiBaseUrl}/purchsaleinvtory/allocation_mulitrow`,
                   //  "http://localhost:3009/purchsaleinvtory/allocation_mulitrow",
                      formDataToSend,
                    {
                      headers: {
                        "Content-Type": "multipart/form-data",
                      },
                    });

                  if(res.status === 200){

                      successCount += chunk_bat.length;

                      setUploadProgress(
                        Math.round(((i+chunk_bat.length)/totalCount)*100)
                      );

                      if(successCount >= totalCount -1 ){
                          // ✅ 成功全部分配後將當前pumpup關閉              
                          toast.success(`分配採購序序號完成,一共配發${final_allocate_packet}包!`); 
                          handleCancel();
                      }
                  }
            }                                

        } catch (error) {
            if (axios.isCancel(error)) {
              console.log("🚫 request 被取消");
            } else {
              console.error("Error post convert_json_formData:", error);
            }        
        } finally{
           setLoading(false);
        }
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
                      .slice(1, 8)
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
                <input type="checkbox" 
                       id="switch"                       
                       checked={istaskallocate_mode}                       
                       onChange={(e) => setTaskAllocate_Mode(e.target.checked)}
                />
                <label className="switch-label" htmlFor="switch">
                    <span className="switch-txt"
                          turn_direct="一般通用" 
                          turn_allocate="生產用料"                                                                             
                     >    
                    </span>
                </label>
                <p style={{alignItems:"center", fontSize:"1.0rem" , fontStyle:"oblique"}}>模式|Mode</p>                                                  
                <div className="warehouse-group">
                  <label >庫別:
                      <select
                        name="warehouse"
                        style={{marginRight:"5px" , transform: "translateX(5px)" , backgroundColor:"rgba(240, 240, 221, 0.73)" }}
                        value={warehouse_label}                        
                        onChange={handle_Change}
                      >			   
                        {
                          storeitems.map((it, idx) => (
                            <option key={it.id} value={it.name}>
                              {/* {`${idx+1}`}.{it.name} */}
                              {it.name}
                              </option>
                          ))
                        }
                      </select>	
                  </label> 
                  <span className="">倉位:
                  <select
                        name="stack_position"
                        style={{
                          width: "79%",
                          paddingleft: "100px",
                          fontSize: "15px",
                          paddingRight: "10px", 
                          borderRadius: "4px", 
                          backgroundColor:"rgba(240, 240, 221, 0.73)"              
                        }}
                        value={stackposition_label}
                        onChange={handle_Change}
                      >			   
                        {
                          locationitems.map((it, idx) => (
                            <option key={it.id} value={it.name}>
                              {/* {`${idx+1}`}.{it.name} */}
                              {it.name}
                              </option>
                          ))
                        }
                      </select>	
                  </span>
                  {/* <input                    
                      name="stack_position"
                      placeholder="輸入倉位(英文含數字)"                                                                    
                      style={{
                          width: "39%",
                          paddingleft: "100px",
                          fontSize: "15px",
                          paddingRight: "10px", 
                          borderRadius: "4px",                
                      }}
                      type="text"                          
                      value={stackposition_label}			 
                      onChange={handle_Change}
                    /> */}
                      
                </div>                        
              </div>
               {/* 當單位為pcs 以下需要做重量設定在往下一步*/}
               {  adjust_unit_refix && istaskallocate_mode &&           
                    <div
                          className="mb-3"
                          style={{
                            display: "flex",                              
                            alignItems: "center",                                                                          
                            backgroundColor: "#c8efff",
                            padding: "20px 10px",                            
                            justifyContent: "center",   // ⭐ 關鍵：整體置中
                            gap: "20px",                 // 每個元件間距                                        
                            flexWrap: "wrap",         // 螢幕太小時自動換行
                            marginTop: "25px",
                            marginBottom: "30px",
                            margin: "20px 10px 0px",                     
                          }}                      
                     >
                        <label style={{fontSize:"1.7rem" , marginRight: "30px"}}>
                          <input
                            name="pass_check"
                            type="checkbox"
                             style={{
                              transform: "scale(1.5)",   
                              marginRight: "8px",
                            }}                         
                            checked={check_iqc_mode === "Pass"}
                            onChange={handle_Change}
                            // onChange={() => setCheckIQC_MODE(check_iqc_mode === "Pass" ? "" : "Pass")}
                          />
                          全良品
                        </label>
                       <label style={{fontSize:"1.7rem"}}>
                          <input
                            type="checkbox"
                             style={{
                              transform: "scale(1.5)",    
                              //marginRight: "8px",
                            }}
                            checked={check_iqc_mode === "Error"}
                            onChange={handleCheckIssueChange}
                          />
                          有異常
                        </label>
                        {check_iqc_mode === "Error" && isIssuecheck &&
                          <div 
                              style={{
                                  display: "flex",
                                  alignItems: "center",                                  
                                  gap: "10px",
                                  flexWrap: "nowrap",     // ⭐ 不換行關鍵
                                  flexShrink: 0,
                                  whiteSpace: "nowrap",                              
                              }}
                          >
                            <span style={{font:"caption", fontSize: "20px", whiteSpace: "wrap"}}>
                              檢驗異常數量:
                            </span>
                            <input
                              type="number"
                              style={{
                                marginLeft: "10px",
                                width: "100px",
                                padding: "8px 10px",
                                fontSize: "20px",
                                alignItems:"center",
                                color:"rgb(22, 22, 17)",
                                backgroundColor:"rgb(231, 116, 121)"
                              }}
                              value={check_error_pcs} 
                              min={0}
                              onChange={(e) =>
                                setCheck_Error_PCS(Number(e.target.value))
                              }
                              ></input>
                              <Select
                                multiple
                                value={error_status}
                                onChange={handleError_list_SelectChange}
                                displayEmpty
                                sx={{
                                  minWidth: 220,
                                  height: 42,
                                  flexShrink: 0,
                                }}
                                renderValue={(selected) => {
                                  if (isIssuecheck && selected.length === 0) {
                                    return (
                                      <span style={{ color: "#999" }}>
                                        請選擇異常原因 
                                      </span>
                                    );
                                  } else return selected.join(", ");
                                }}
                              >                      
                              {Issue_all_Options.map((item , index) => (
                                <MenuItem
                                  key={item}
                                  value={item}                                  
                               //   onChange={() => toggleGroup(item)}                                                                    
                                >
                                  <Checkbox checked={error_status.indexOf(item) > -1} />                                  
                                  <ListItemText
                                  primary={`${item}`}
                                  />
                                </MenuItem>
                              ))}
                           </Select> 
                           {/*如果檢驗異常數量是大等於1以上,下面開啟選擇NG文件附加啟動*/}
                           {ismustattached && 
                              <div style={{display:"inline-block",alignItems:"center",gap:"30px"}}>
                                <label className="TitleName" htmlFor="file-upload">
                                    NG文件上傳 :
                                </label>
                                <label
                                    className="TitleName"
                                    htmlFor="file-upload"
                                    style={{ fontSize: "16px", color: "red", fontWeight: "900"}}
                                >
                                    *文件上傳限制10mb : 
                                </label>
                                <Form.Control
                                    type="file"
                                    multiple
                                    onChange={handleFileChange}
                                    accept=".pdf, .jpg, .jpeg, .png ,.tiff , .bmp"
                                    ref={fileInputRef}
                                    
                                  />
                                  {file.length > 0 && (
                                    <div className="mt-3"                                         
                                    >
                                      <h5>選擇上傳文件:</h5>
                                      <ul>
                                        {file.map((fileItem, index) => (
                                          <li
                                            key={index}
                                            style={{                                                                  
                                              display: "flex",
                                              gap: "5px",              // 每個元件間距
                                              flexWrap: "nowrap",         // 不換行
                                              marginTop: "5px",
                                            }}
                                          >
                                            {getFileIcon(fileItem.name)} {fileItem.name} (
                                           {`檔案:${index+1}:`} {(fileItem.size / (1024 * 1024)).toFixed(2)} MB)
                                          </li>
                                        ))}
                                      </ul>
                                      <p style={{ fontWeight: "bold" }}>
                                          總檔案大小：{totalMB} MB（
                                          <span style={{ color: isOverLimit ? "red" : "green" }}>
                                            {isOverLimit ? "已超過限制 10MB" : "可上傳"}
                                          </span>
                                          ）
                                      </p>
                                    </div>
                                  )}
                                </div>
                            }
                           </div>                         
                         }
                        {check_iqc_mode !==""  && 
                          <div
                            style={{
                              display: "flex",
                              alignItems: "center",
                              textAlign:"center",
                              gap: "10px",
                              flexWrap: "wrap",
                              flexShrink: 0,
                            }}
                          >
                         <label htmlFor="allcate_code">
                           <strong style={{ fontSize: "26px" , ...(check_iqc_mode === "Error" ? {} : { marginLeft: "10px" }),}}>請先設置{unit_fields}單位→</strong>
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
                            width: "110px",
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
                  </div>          
                }

                {allocate_need &&   
                  istaskallocate_mode &&
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
                  { !istaskallocate_mode &&
                    <div className="radio-container">
                      <label className="radio-item">
                        <input
                          type="radio"
                          name="normal_method"
                          value={allocatecase.normal_method}
                          checked={radiomethod === "normal"}
                          onChange={handle_Change}
                        />
                        <p className="dbselect">一般檢驗入倉</p>
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
                 {loading  && (
                    <div 
                          style={{  display: "flex",
                                   flexDirection: "column",
                                   alignItems: "center",
                                   gap: "12px",
                                   padding: "20px"
                          }}
                    >
                      <Skeleton height={200} style={{ marginBottom: 2 ,borderRadius: 8 ,width: "150%"} } baseColor="#475ac0" highlightColor="#e7eec6" animation="wave"  />                
                      {/* <Skeleton height={50} baseColor="#2c8f96" highlightColor="#ebec95" animation="wave" /> */}
                      {/* 旋轉大區塊 Skeleton */}
                      <div className="spinner-wrapper">
                        <div className="loading-spinner"></div>
                          <Skeleton
                              circle
                              width={50}
                              height={50}
                              baseColor="#10daa7"
                              style={{
                                position: "absolute"
                              }}
                          />
                      </div> 
                        資料提交中 {uploadProgress}%                 
                    </div>                    
                   ) 
              } 

               </div>
                {/* prorated 平均自動分配*/}
                {istaskallocate_mode && radiomethod === "prorated" && (
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
                        width: "100px",
                        padding: "8px 10px",
                        fontSize: "20px",
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
                 {istaskallocate_mode  && radiomethod === "mannul" && (  
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
                                fontSize: "20px",
                                fontWeight: "bold",
                                padding: "2px 10px",
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
          {/* normal 一般分配 */}
		      {!istaskallocate_mode && radiomethod === "normal" && (  
           <div
              style={{
                display: "flex",
                 alignItems: "center",
                 justifyContent: "center",
                 gap: "40px",             
              }}
           >
            <div
                style={{
                  width: "103%",
                  marginTop:"20px",
                  border: "1px solid #ccc",
                  borderRadius: "8px",                          
                  marginBlock: "inline-block",                          
                  background: "#f7f7f7",
                  margin: "10px 30px",
                }}
            >
              {/* 分配PCS */}
              <div
                style={{
                  display:"flex",
                  //justifyContent: "center", // 整體置中
                   alignItems: "center",     // 垂直置中
                  gap: "5px",              // 每個元件間距
                  flexWrap: "wrap",         // 螢幕太小時自動換行
                  marginTop: "25px",
                  marginBottom: "30px",
                  margin: "20px 10px 0px",
                }}
              >
                <span style={{font:"caption", fontSize:"20px"}}>
                  入倉檢驗合格數量:
                </span>
                <input
                      type="number"
                      style={{
                        marginLeft: "10px",
                        width: "100px",
                        padding: "8px 10px",
                        fontSize: "20px",
                        alignItems:"center",
                        color:"rgb(22, 22, 17)",
                        backgroundColor:"rgb(156, 252, 193)"
                      }}
                      value={allocatePCS}                          
                      min={1}
                      max={10000}
                      onChange={(e) =>
                        setAllocatePCS(Number(e.target.value))
                      }
                />                  
                <span style={{font:"caption", fontSize:"20px"}}>
                  檢驗異常數量:
                </span>
                <input
                  type="number"
                  style={{
                    marginLeft: "10px",
                    width: "100px",
                    padding: "8px 10px",
                    fontSize: "20px",
                    alignItems:"center",
                    color:"rgb(22, 22, 17)",
                    backgroundColor:"rgb(231, 116, 121)"
                  }}
                  value={check_error_pcs} 
                  min={0}
                  onChange={(e) =>
                    setCheck_Error_PCS(Number(e.target.value))
                  }
                />
                {/*如果檢驗異常數量是大等於1以上,下面開啟選擇NG文件附加啟動*/}
                {ismustattached && 
                  <div style={{display:"flex",alignItems:"center",gap:"30px"}}>
                    <label className="TitleName" htmlFor="file-upload">
                        NG文件上傳 :
                    </label>
                    <label
                        className="TitleName"
                        htmlFor="file-upload"
                        style={{ fontSize: "16px", color: "red", fontWeight: "900"}}
                    >
                        *文件上傳限制10mb : 
                    </label>
                    <Form.Control
                        type="file"
                        multiple
                        onChange={handleFileChange}
                        accept=".pdf, .jpg, .jpeg, .png ,.tiff , .bmp"
                        ref={fileInputRef}
                        
                      />
                      {file.length > 0 && (
                        <div className="mt-3">
                          <h5>選擇上傳文件:</h5>
                          <ul>
                            {file.map((fileItem, index) => (
                              <li
                                key={index}
                                style={{                                                                  
                                  display: "flex",
                                  gap: "5px",              // 每個元件間距
                                  flexWrap: "nowrap",         // 不換行
                                  marginTop: "5px",
                                }}
                              >
                                 {getFileIcon(fileItem.name)} {fileItem.name} (
                                {(fileItem.size / (1024 * 1024)).toFixed(2)} MB)
                              </li>
                            ))}
                          </ul>
                          <p style={{ fontWeight: "bold" }}>
                              總檔案大小：{totalMB} MB（
                              <span style={{ color: isOverLimit ? "red" : "green" }}>
                                {isOverLimit ? "已超過限制 10MB" : "可上傳"}
                              </span>
                              ）
                          </p>
                        </div>
                      )}
                    </div>
                   }
                  </div>                  
                   <div  
                      style={{
                          maxHeight: "400px", // 最大高度
                          overflowY: "auto", // 超過出現捲軸
                          overflowX: "hidden",
                          paddingRight: "8px",
                           marginRight:"12px"
                        }}
                      >       
                        <span 
                         style={{
                              display: "block",
                              width: "fit-content",
                              margin: "5px auto",
                              fontSize: "3rem",
                              fontWeight: "bold",
                              borderBottom: "2px solid #71cfc7",
                              paddingBottom: "8px",
                          }}                          
                        >通用編碼 
                        </span>

                        {allocate_dataRows.map((row, idx) => (
                          <div 
                              style={{
                                display: "flex",                                
                                gap: "20px",                                
                                borderBottom: "1px solid #71cfc7",
                                // justifyContent: "space-between", // 平均分配左右空間
                                alignItems: "center",
                                gap: "20px",
                                padding: "12px 16px",
                                marginBottom: "1px solid #71cfc7", // 每個 row 間距
                              }}
                              key={idx}
                          >                        
                            {/* 左側日期區塊 */}
                            <span
                              style={{
                                width:"920px",
                                fontSize: "22px",
                                fontWeight: "bold",
                                padding: "10px 30px",
                                borderRadius: "8px",
                                background: "rgba(0,0,0,0.12)", // 遮罩感
                                backdropFilter: "blur(4px)", // 毛玻璃效果
                                WebkitBackdropFilter: "blur(4px)",
                                boxShadow: "0 2px 6px rgba(213, 216, 178, 0.15)",
                                color: "#020408",                               
                              }}
                            >
                              第{idx + 1}筆: {row.date_stage_code}
                            </span>
                            {/* 右側重量 */}
                              <span
                                style={{
                                  fontSize: "18px",
                                  fontWeight: "60px",
                                  color: "#134e4a",
                                  flex: 1,
                                  textAlign: "right",
                                  paddingRight:"20px"
                                }}
                              >
                                {row.inputValue||""}
                                {" "}({unit_fields})
                              </span>                       
                          </div>
                        ))}
                     </div>
                  </div>             
                </div>
		          )
         }

         {modalIsOpen && allocaTarget ? (
            <Suspense fallback={<div>Loading...</div>}>
              <Confirm_AllocationModal
                show={modalIsOpen}
                onHide={handle_Allocation_OnHide}
                centered={true}
                distribute_info={allocaTarget}
              />
            </Suspense>
          ) : null} 
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
      </Modal>
  );
}

export default AllocationPopup_Work;