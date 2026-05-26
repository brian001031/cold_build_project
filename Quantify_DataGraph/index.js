import "./index.scss";
import debounce from "lodash/debounce";
import React, { useState, useEffect, useRef, useMemo ,useCallback } from "react";
import { Button, Table, InputGroup, DropdownButton, Dropdown, Toast } from 'react-bootstrap';
import { createPortal } from 'react-dom';
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Form from "react-bootstrap/Form";
// eslint-disable-next-line no-unused-vars
import config from "../../config";
import * as echarts from "echarts/core";
import { Advancedselect_trigger } from "../../components/AdvancedSelectTrigger";
import { isArray, kebabCase } from "lodash";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import 'moment/locale/zh-tw'; 
import { FormattedMessage, useIntl } from "react-intl";
//成功提示套件
import { toast } from "react-toastify";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import {group_direct_sulting_fields} from "../../mes_remak_data";

// 導入 MessagePopup 組件
import MessagePopup from '../../components/MessagePopup';


import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent
} from 'echarts/components';

import {
  LineChart,
  BarChart
} from 'echarts/charts';

import { CanvasRenderer } from 'echarts/renderers';
import { color } from "echarts";


echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  LineChart,
  BarChart,
  CanvasRenderer
]);

// const cc_list = ["CC1","CC2"]
const cc_list = ["017","010"]
const cc_captype_list = ["CC1","CC2"]

const placeholder_cellstr_only =  ["全部資料" ,"分選" ]
const filter_title = ["MachineStatusCode","OPNO"];

const sulting_options = [
  { value: "sulting_pf", label: "分選化成" },
  { value: "sulting_cc", label: "分選分容" },
];

const progess_time_base = Number(200);

const COLOR_MAP = {
  CC1: "#c7655e",
  CC2: "#4796e0",
  CC3: "#FAC858",
  default: "#999999",
};


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


  //下列為當切換全年月日數據查詢所需要用到的元件和存取變數區
  const [isfullCapdata, setFull_Capdata] = useState(true);
  const [serialopen, set_SerialAlwaysOpen] = useState(true);
  const [inputPage, setInputPage] = useState(""); // 新增輸入頁狀態
  const [pageSize, setPageSize] = useState(20); // 每頁顯示 20 筆
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [loading2, setLoading2] = useState(false);
  const [progress, setProgress] = useState(0);
  const [option_cap, setOption] = useState('全部資料');
  const [searchTerm, setSearchTerm] = useState("");
  const MIN_DATE =  moment("2024-01-01");  // 2024-01-01 預設
  const defaultStartDate =  moment().locale("zh-tw").startOf("year"); //今年度1月1號
  // 取兩者較早的日期
  const finalStartDate = moment.min(defaultStartDate, MIN_DATE);
  const [startDate, setStartDate] = useState(finalStartDate); //目前預設使用2024年度第一天
  const [endDay, setEndDay] = useState(moment().locale("zh-tw"));
  const [data_info, setDataAll_info] = useState([]);
  const [data_excel_info, setDataAll_excelinfo] = useState([]);
  const [ all_percent_capacity, setAll_Percent_Capacity] = useState([]); //保存當前前綴序號分容型態與數量
  const [csvUrl, setCsvUrl] = useState(null);
  const [sultingquery, setSultingQuery] = useState({
      keyword: '',
      cap_side: ''
  });

  //下列為實現級距(bar , pie-line)圖形數據
  const chartRef = useRef(null); // 创建 ref(bar) 来引用 DOM 元素
  const chartRef2 = useRef(null); // 创建 ref2(pie-line) 来引用 DOM 元素
  const chartRef_3_radio = useRef(null); // 创建 ref2(pie-line) 来引用 DOM 元素
  const chartInstanceRef = useRef(null);
  const chartReadyRef = useRef(false);
  const navigate = useNavigate();

  //新增分選站別選單
   const [selectedOption, setSelectedOption] = useState("sulting_cc");
   const [isprefixLoading, setIsprefixLoading] = useState(false);

   const getprefix_Width = (text) => {
      return `${text.length * 2.1 + 2}ch`;
  };

 // 一開始先接收目前電芯號前綴序號別名(just do one times)
  useEffect(() => {

    if(!sideoption || !selectedOption){
       console.error("無法擷取MES站點名稱 side-options:", sideoption); 
       console.error("無法擷取分選類別名稱 selectedOption:", selectedOption); 
       return ;
    }
    
    //page 登入將所有電芯年份前綴先收集呈現
    const fetch_modelId_prefix_list = async (side_name) => {

      setIsprefixLoading(true);

      try {
            const res = await fetch(
           // `http://localhost:3009/scatterdigram/model_prefixlist?sidename=${side_name}&sultingcase=${selectedOption}`,
              `${config.apiBaseUrl}/scatterdigram/model_prefixlist?sidename=${side_name}&sultingcase=${selectedOption}`,
            );

          if (!res) throw new Error(`無擷取相關-> ${side_name}站電芯序號資訊!`);

          const result = await res.json();

          // console.log("目前接收 list 清單為= " + JSON.stringify(result,null,2));

          //清空列表單
          setSerial_prefixlist([]);
          setOption('全部資料');
          
          if (res.status === 200 && result.data)
          {
            // console.log("接收回傳存取進行中..");            
            const serial_options = result.data.map((item, index) => ({
              prefix: item.model_prefix,
              num: index
            }));

            setSerial_prefixlist(serial_options);

            //清除當前數據資料內容
            setDataAll_info([]);
          }

      } catch (error) {
          console.error("Error fetching options:", error);
          return "";
      }finally{
        setIsprefixLoading(false);
      }
   };

   fetch_modelId_prefix_list(sideoption);
   
 },[selectedOption, sideoption]);



//  useEffect(() => {
//   const dom = chartRef_3_radio.current;
//   if (!dom) return;

//   const ro = new ResizeObserver(() => {
//     const instance = chartInstanceRef.current;
//     if (instance) {
//       instance.resize();
//     }
//   });

//   ro.observe(dom);

//   return () => ro.disconnect();
// }, []);


useEffect(() => {
   const dom = chartRef_3_radio.current;
   if (!dom) return;

   let instance = echarts.getInstanceByDom(dom);
   
   // fullCapdata → destroy chart
   if (isfullCapdata) {    
    instance?.dispose();
    chartInstanceRef.current = null;
    return;
   }

  
    if (!instance) {
       instance = echarts.init(dom, "dark");
       chartInstanceRef.current = instance;
    }

    const resize = () => instance.resize();
    window.addEventListener("resize", resize);


    return () => {
      window.removeEventListener("resize", resize);
    };
   
}, [isfullCapdata]);

//  const column_list = useMemo(() => {
//   const group = Object.values(group_direct_sulting_fields)[0];
//   const labelMap = group?.[0] || {};
//   return Object.keys(labelMap).filter((r)=>{
//     return !filter_title.includes(r);
//   } );
//  }, []);

 const getSelectedGroup = (option) => {
  const group = group_direct_sulting_fields.waiting_for_group_name;

  switch(option){
    case "sulting_cc":
      return group[0];
    case "sulting_pf":
      return group[1];
    default:
      return {};
  }
};

//不使用flamap ,直接Object取index 回傳, column_list 改變時，table structure 沒被強制刷新,所以這邊用useMemo需要再次刷新
const column_list = useMemo (()=>{

  const group = getSelectedGroup(selectedOption);

  return Object.keys(group).filter(
    key => !filter_title.includes(key)
  );

}, [selectedOption, filter_title]);



  // console.log("目前Suliting 化成分容 欄位list = " + JSON.stringify(column_list,null,2));

//取得當前分選(PF或CC)表單header!
// const column_list = Object.values(group_direct_sulting_fields)
//   .flatMap(group => Object.keys( selectedOption === "sulting_cc"?group?.[0]
//                                  :selectedOption === "sulting_pf"?group?.[1]
//                                  :{}))
//   .filter(key => !filter_title.includes(key));


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


    //直接擷取單前前綴電芯序號(找出目前生產(cc1,cc2)總比例分配)

    const fetch_modelId_cap_Percentage = async () => {
      try {
            const res = await fetch(
           //  `http://localhost:3009/scatterdigram/sultin_CapPercent?prefixname=${selected_serial}`,
            `${config.apiBaseUrl}/scatterdigram/sultin_CapPercent?prefixname=${selected_serial}`,

            );

          if (!res.ok) throw new Error(`API error: ${res.status} 無擷取相關-> ${selected_serial}電芯前綴虛耗電容資訊!`);

          const result = await res.json();

          // console.log("raw result =", result);
 
          // console.log("目前接收 分選 CC1, CC2  數量list 清單為= " + JSON.stringify(result.cap_total,null,2)); 
      
          
          if (res.ok && result.cap_total)
          {
            //先清空暫存區
            setAll_Percent_Capacity([]);

            const serial_percent_info = result?.cap_total?.[0] ?? {};               
            const sulting_all_percent_info = Object.entries(serial_percent_info).map(([key, value],index) => ({
              radio_num: value,
              cap_type:  key.replace("_count", ""),              
            }));   
            
            console.log("目前接收 分選 CC1, CC2  數量list 清單為= " + JSON.stringify(sulting_all_percent_info,null,2));           
            setAll_Percent_Capacity(sulting_all_percent_info);
          }

      } catch (error) {
          console.error("Error fetching options:", error);
          return "";
      }
   };
   
   fetch_modelId_cap_Percentage();

  }, [selected_serial]); // 當 selected_serial 改變時觸發

  const handleChange =  (e) => {
    const { name, value } = e.target;
  
    if( name === "prefix"){
      setSelected_serial(prev => value +"");      
    }
   
  };

  const handleToggle = () => {
	  setFull_Capdata((prev) => !prev);
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

   const goToPage = (value) => {
 
    // 防止超出範圍
    // page > totalPages
    //   ? setPage(totalPages)
    //   : page < 1
    //   ? setPage(1)
    //   : setPage(page);

    const num = Number(value);
    if (!num || isNaN(num)) return;

    const safePage = Math.min(Math.max(num, 1), totalPages);

    setPage(safePage); // Update the page state
    setInputPage(""); // 清空 input
 
  };

  const prevPage = () => setPage((p) => Math.max(p - 1, 1));
  const nextPage = () => setPage((p) => Math.min(p + 1, totalPages));


  const get_color_param = ( cc_list=[]) => {

    const  CC_Array_Name =  !Array.isArray(cc_list)?[]:String(cc_list).split(',');


  }

  //執行電芯分選條件查詢
  const fetch_sultingdata = async (page = 1 ,query = sultingquery , sult_side = selectedOption ) => {
      setLoading(true);
						
			//將要查詢的param 組包 json 
			const request_param =
			{
        cc_serial:selected_serial,
			  numpage: page,
			  page_Size:pageSize,
			  keyword: query.keyword,  
			  cap_side: query.cap_side,  
			  stDate: moment(startDate).format("YYYY-MM-DD"),
			  edDate: moment(endDay).format("YYYY-MM-DD"),
			  sortOrder: 'desc',
        sulting_side : sult_side
			};
      
      //增加延遲閃時間
      const start = Date.now();

      try {
            const res = await axios.post(
              `${config.apiBaseUrl}/scatterdigram/get_cellinfo_fromSulting`,
              //  "http://localhost:3009/scatterdigram/get_cellinfo_fromSulting",
                 request_param,
                {
                  headers: {
                      "Content-Type": "application/json"
                  },
                }
			      );

            
            const res_all = res.data;
            // console.log("得到cell總SQL query = "+ JSON.stringify(res_all.count_sql,null,2))
            // console.log("得到cell總ROW為 = "+ JSON.stringify(res_all.result_allinfo,null,2))
            // console.log("得到cell總各個PAGE ,size  = "+ JSON.stringify(res_all.view_param,null,2))
            // console.log("得到總資料查詢quety SQL data最終結果= "+ JSON.stringify(res_all.final_sql_result,null,2))

            setDataAll_info(res_all.final_sql_result);

            Object.entries(res_all.view_param).forEach(([key, value],index) => {
              // console.log(`- ${key}:`, value, `(${typeof value})`);
              if( key==="totalPages")setTotalPages(value);
              //useEffect 監聽 page，就不能在 fetch 裡再 setPage
              //if(key==="page")setPage(value);
            });

        } catch (err) {
            console.error(err);
        } finally {
            const diff = Date.now() - start;
            const delay = Math.max(300 - diff, 0);

            setTimeout(() => {
              setLoading(false);
            }, delay);
        }
    };

  const handleSearch = (e) => {
        e.preventDefault();
  
        // 防止選到開始日期或結束日期不符合順序的卡控
        if (moment(startDate) > moment(endDay)) {
            toast.error(`開始日期:${moment(startDate).format("YYYY-MM-DD")}不能比結束日期晚!`);
            return;              
        }else if (moment(endDay) < moment(startDate)) {
            toast.error(`結束日期:${moment(endDay).format("YYYY-MM-DD")}不能比開始日期早!`);
            return;
        }
  
  
        //當切換電容量查詢且keyword 輸入非整數或非浮點數
        // if(String(option.trim()).includes("電容量級距查詢") && (isNaN(searchTerm))){
        //     toast.error(`電容量級距查詢關鍵字:${searchTerm}不能是非數字`);
        //     return;        
        // }
        
        const side_option =
        String(option_cap.startsWith("CC1") && option_cap.trim()).includes("未分選")
        ? "CC1"
        :  String(option_cap.startsWith("CC2") && option_cap.trim()).includes("32分選")
        ? "CC2"
        : String(option_cap.trim());
	
	
	      // 合并 keyword 和 cap_side 更新
        // setSultingQuery({
        //     ...sultingquery, // 保留之前的值
        //     keyword: searchTerm, // 更新 keyword
        //     cap_side: side_option // 更新 station
        // });

        const newQuery = {
            keyword: searchTerm,
            cap_side: side_option
          };

        setSultingQuery(newQuery);

         // 🔥 重點：直接打 API（不要等 useEffect）
         fetch_sultingdata(1, newQuery ,selectedOption);
        
        setPage(1); // // 搜尋時回到第1頁
  };


  const ReflashPie_radio_Chart = useCallback((percent_data = []) => {
    if (!Array.isArray(percent_data) || percent_data.length === 0) return;

    if (!chartRef_3_radio.current) {
      console.warn("DOM 尚未準備好");
      return;
    }    
    
    let myChart = chartInstanceRef.current;
     
     //如果還是沒有，且 DOM 存在，就地初始化 (保險做法)
     if (!myChart )  {
        myChart = echarts.getInstanceByDom(chartRef_3_radio.current);
     }

     // still not exist → 初始化（關鍵修正）
     if (!myChart) {
        myChart = echarts.init(chartRef_3_radio.current,"dark");
        chartInstanceRef.current = myChart;
      }

 
   
     console.log("檢查是否已經存在圖表實例，避免重複創建  = "+ myChart);

    //取得type
    //const capItem = Object.values(percent_data).map(col=> col.cap_type);
    //取得分容數量
    // const capcount = Object.values(percent_data).map(col=> col.radio_num);

    // console.log("目前要轉換之分容資訊為capItem項目:"+ capItem + "   佔有量capcount:" +capcount);

    //  const safe_renderData = Array.isArray(percent_data)
    //                       ? percent_data
    //                       : Object.values(percent_data || []);


                          
     const safe_renderData = percent_data.map(i => ({
        value: Number(i.radio_num),
        name: i.cap_type,
    }));

    const total_cc_all = percent_data.reduce((sum, item) => {
      return sum + Number(item.radio_num);
    }, 0);

    console.log("CC1 CC2總量為:"+total_cc_all);
  
    // console.log("safe_renderData = "+ JSON.stringify(safe_renderData,null,2));

    
    //  console.log("height =", chartRef_3_radio.current?.clientHeight);
    //  console.log("canvas dom:", chartRef_3_radio.current.querySelector("canvas"));
    

    try {

      const option = {
               title: {
                text: `${selected_serial}-分容產量柱狀圖`,
                left: "center", // 可選：讓標題置中
                top: 10, // 可選：調整上下位置
                textStyle: {
                  fontSize: 28,
                  fontWeight: "bold",
                },
              },

               emphasis: {
                focus: "series",
              },                                      
              tooltip: {
                  trigger: 'axis', 
                  backgroundColor: "rgba(195, 243, 186, 0.9)",
                  borderColor: "#4b1d21",
                  borderWidth: 5,
                  textStyle: {
                    color: "#0d160c",
                    fontSize: 20,
                  },           
                  formatter: (params) =>{
                      // console.log("全部params = " + JSON.stringify(params,null,2));                       
                      const first = params[0];
                      const cap_mah_value = Number(first?.value || 0);
                      const determine_case = first?.name?.includes("CC2") ? "32分選" : "未分選";
                      const radio_percent = Math.round((cap_mah_value / Number(total_cc_all)) * 10000) / 100;

                       return (
                        params
                          .map(
                            (p) =>
                              `${p.seriesName}${p.name}-生產量: ${p.value} (Qty)`
                          )
                          .join("<br>") +
                        "<br>" +
                        `電芯總產量: ${total_cc_all} (Qty)` +
                        "<br>" +
                        `${determine_case}-占總比例: ${radio_percent} %`
                      );
                  }
              },
              

              // legend: {
              //   data: capItem
              // },

              // legend: {
              //   type: "scroll",
              //   orient: "vertical",
              //   left: "left",
              //   data: safe_renderData.map(i => i.cap_type),
              //  //  top: "bottom",
              // },

               xAxis: {
                  type: "category",
                  data: safe_renderData.map(i => i.name),
                },

                yAxis: {
                  type: "value", 
                  name: "Qty",
                  nameTextStyle: {
                    fontSize: 16,
                    fontWeight: "bold",  
                    align: "left"                   
                  },                  
                  nameLocation: "end",                 
                  nameGap: 12,
                  axisLabel: {
                    color: "#ddd",
                    formatter: (value) => {
                      if (value >= 1000000) return (value / 1000000).toFixed(1) + "M";
                      if (value >= 1000) return (value / 1000).toFixed(0) + "K";
                      return value;
                    }
                  },
                  splitLine: {
                    lineStyle: {
                      type: "dashed"
                    }
                  }
                },

              series: [           
                  {
                    name: "分容",
                    type: "bar",
                    radius: ["35%", "60%"],
                    avoidLabelOverlap: true,
                    data: safe_renderData.map(i => ({
                      value: i.value,                      
                      itemStyle: {
                        color: COLOR_MAP[i.name] || "#1db336"
                      }
                    })),                  
                    barWidth: "45%",
                    label: {
                      show: true,
                      position: "top",
                    },
                    animationType: 'expansion',
                    animationDuration: 1000
                },
            ],
       };

      //  console.log("option =", option);
      //  console.log(JSON.stringify(option.series, null, 2));

         
      // 設定圖表選項             
       myChart.setOption(option, true);
                  
        // ⚠️ delay resize（避免 DOM 還沒 ready）
        // 確保容器大小正確
        setTimeout(() => {
            myChart.resize();
        }, 100);


    }catch (error) {
      console.error("取得資料錯誤", error);
    }
  }, [selected_serial]); // 依賴項加入序號，當序號變動時重新產生函數


    useEffect(() => {
      
      fetch_sultingdata(page);
    
    }, [page, pageSize]);


  useEffect(() => {


      //  if (!all_percent_capacity?.length) return;

      // 🔥 等 chart ready
      // if (!chartInstanceRef.current) return;
      
      //重新渲染pie 百分比分容圖表
      ReflashPie_radio_Chart(all_percent_capacity);

  }, [all_percent_capacity]); // 移除setDataAll_info依賴

    const getProgressColor = (progress) => {
      if (progress < 45) return "#ec5454";   // 紅
      if (progress < 80) return "#aaffde";   // 橘黃
      return "#15a156d0";                     // 綠
    };

  //v download下載->後端執行完EXCEL 的檔案於操作電腦環境下
  const download_XLS_File = async (taskId) => {

      const url = //`http://localhost:3009/scatterdigram/exportdownload/${taskId}`;
                `${config.apiBaseUrl}/scatterdigram/exportdownload/${taskId}`;

      //--------精簡版--------------------
      // const link = document.createElement("a");
      // link.href = url;
      // // link.setAttribute("download", `${selected_serial}_${sultingquery.keyword}_${sultingquery.cap_side}.xlsx`);
      // link.setAttribute("download", "export.xlsx");
      // document.body.appendChild(link);
      // link.click();
      // link.remove();

      // setTimeout(() => {
      //   setLoading2(false);
      //   setProgress(0);
      // }, 1500);
      //------------------------end----------------

      const res = await fetch(url);
      const blob = await res.blob();

      const link = document.createElement("a");
      const objectUrl = window.URL.createObjectURL(blob);

      link.href = objectUrl;
      link.download = "export.xlsx";

      document.body.appendChild(link);
      link.click();

      link.remove();
      window.URL.revokeObjectURL(objectUrl);

      setLoading2(false);
      setProgress(0);
  }
  

  //監控進度百分比數值
  const listenProgress = (taskId) => {
    
      const es = new EventSource(
       `${config.apiBaseUrl}/scatterdigram/progress_taskID/${taskId}`,
      // "http://localhost:3009/scatterdigram/progress_taskID/${taskId}"
       );

      es.onmessage = (e) => {
          const data = JSON.parse(e.data);
           const process_num = data.progress;
          setProgress(process_num);

          if (process_num >= 100) {
            es.close();
            download_XLS_File(taskId);
          }
      };
  }


  const startExport = async (e) => {
      e.preventDefault();
      setLoading2(true);
      setProgress(0);

      const res = await axios.get(`${config.apiBaseUrl}/scatterdigram/export_taskID`);      
      // const res = await axios.get("http://localhost:3009/scatterdigram/export_taskID");

      const taskId = res.data.Task_ID;
      // console.log("得到cell總共資訊為 = "+ JSON.stringify(res.data,null,2))
      listenProgress(taskId);
  };

    //匯出EXCEL (使用假進度)
    const exportCSV_NotrueGroess = async (e) => {
      e.preventDefault(); //  防止 form / input 重送

      //開始進度表值出約設
      setProgress(0);
      setLoading2(true);
   
      try {
            const res = await axios.get(
               `${config.apiBaseUrl}/scatterdigram/export_excel_sulting`,
               //  "http://localhost:3009/scatterdigram/export_excel_sulting",                 
                 {
                    params: {
                      cc_serial:selected_serial,
                      keyword: sultingquery.keyword,  
                      cap_side: sultingquery.cap_side,
                      stDate: moment(startDate).format("YYYY-MM-DD"),
                      edDate: moment(endDay).format("YYYY-MM-DD"),
                      sortOrder: 'desc',
                      sulting_side : selectedOption
                    },     
                }
			      );

            setProgress(90);
            
            const res_all = res.data;
            // console.log("得到export excel 回傳數據流為 = "+ JSON.stringify(res_all.result_excel_allinfo,null,2));
            const data = res_all?.result_excel_allinfo ?? [];

            console.log("回傳結構ˇapi 為"+ JSON.stringify(data,null,2));

            if (data.length === 0 || Number(totalPages) === 0) {
              setDataAll_excelinfo([]);
              console.warn("⚠️ 查無資料");
            }else{
                setDataAll_excelinfo(data);
            } 
                   

            setProgress(100);
        } catch (err) {
            console.error(err);
        } finally {
            setProgress(100);
            setLoading2(false);
        }

      // const rows ={cc1:"1025.521" ,cc2:"5632.105"};
  
      // const worksheet = XLSX.utils.json_to_sheet(rows);
  
      // const workbook = XLSX.utils.book_new();
      // XLSX.utils.book_append_sheet(workbook, worksheet, "配方資料");
  
      // const excelBuffer = XLSX.write(workbook, {
      //   bookType: "xlsx",
      //   type: "array",
      // });
  
      // const blob = new Blob([excelBuffer], {
      //   type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      // });
    
      // const export_label_str = `全32分選紀錄_page_.xlsx`;
      // saveAs(blob, export_label_str);
    };

    const safeDecodeBuffer = (cell) => {
        if (!cell) return "";

        try {
          // Buffer / Node Buffer
          if (typeof Buffer !== "undefined" && Buffer.isBuffer?.(cell)) {
            return cell.toString("utf8");
          }

          // Sequelize / Mongo 常見格式
          if (cell?.type === "Buffer" && Array.isArray(cell?.data)) {
            return Buffer.from(cell.data).toString("utf8");
          }

          if (cell instanceof Uint8Array) {
            return new TextDecoder().decode(cell);
          }

          return cell;
        } catch (e) {
          return "";
        }
    };


    const export_Sulting_ToCSV = (columns, data, filename, delimiter = ",") => {   
            const headers = Array.isArray(columns) ? columns.filter(Boolean) : [];
            const safeData = Array.isArray(data) ? data : [];

            if (headers.length === 0 || safeData.length === 0) {
              console.warn("⚠️ headers 或 data 為空");
              return;
            }

            //先行將不合法或空行過濾掉
            const vaildData = safeData.filter((row, index) => {
              // 過濾條件：row 是物件、且至少一欄有資料（非 null/undefined/空字串）
              if (!row || typeof row !== "object") return false;
               

              // 假設 trayID 在 headers 中的欄位名稱是 trayID
             // const trayID = row["trayID"]; // 根據實際欄位名稱來調整
              // 檢查 trayID 格式是否包含 '-'
             // const trayIDHasDash = typeof trayID === "string" && trayID.includes("-");

              // 檢查 trayID 是否符合兩個字母 + `-` 格式
              // const isTrayIDValid = /^[A-Za-z]{2}-/.test(trayIDPrefix);

              const rowIndexKey = headers[0];
              const rowIndexVal = row?.[rowIndexKey];

              // 條件 1：第一欄為'-'不正常符號row[0] = '-開頭'
              const invalidFirstColValues = ["-", "'-'", "'-CC'"];
              const isInvalidCol0 =
                typeof rowIndexVal === "string" &&
                invalidFirstColValues.some((prefix) => rowIndexVal.startsWith(prefix));

              const isEmptyRow = headers.slice(1).every((key) => {
                const cell = row?.[key];
                return (
                  cell === null ||
                  cell === undefined ||
                  cell === "" ||
                  (typeof cell === "number" && isNaN(cell)) ||
                  (typeof cell === "string" && cell.toLowerCase() === "nan")
                );
              });

              // 如果 trayID 不符合格式，且該行是空行（row[0] === 0 且後續為空），則跳過該行
              if (isInvalidCol0 || isEmptyRow) {
                return false;
              }

              // 檢查是否有至少一個欄位有值（非 null/undefined/空字串）
              // return typeof row === "object" && Object.keys(row).length > 0;
              return headers.some((h) => {
                 const val = row?.[h];
                 return val !== null && val !== undefined && val !== "";
              });                           
            });

            //重整rows 結構,對應column 是否有mapping
            const rows = vaildData.map((row) => 
                  headers.map((h) => {
                 // let cell = row?.[h] !== undefined ? row?.[h] : "";
                  let cell = row?.[h] ?? "";

                  // 處理 Buffer 格式
                  // Buffer / binary safe decode
                  cell = safeDecodeBuffer(cell);

                  // 移除控制字元（保留數據安全）
                  if (typeof cell === "string") {
                    cell = cell.replace(/[\r\n\t]/g, " ");
                  }

                  // CSV escape
                  return typeof cell === "string" && /[",\n]/.test(cell)
                    ? `"${cell.replace(/"/g, '""')}"`
                    : cell;                                  
                })
                .join(delimiter)
            );

            //將欄位 和數據 一起打包
            const csvContent = [headers.join(delimiter), ...rows].join("\n");

            const	 stDate  = moment(startDate).format("YYYYMMDD");
	          const  edDate  = moment(endDay).format("YYYYMMDD");
            const  date_all = stDate+"_"+edDate;
            const  keyword_all = sultingquery.keyword===""?"全部":"包含序號"+sultingquery.keyword.trim();

            //分類站別名稱
            const side_class = selectedOption === "sulting_cc"?sultingquery.cap_side:"pf";
                                                 
            //重新自定義檔案名稱
            filename = `${selected_serial}-${keyword_all}-${side_class}-export-${date_all}.csv`;
            

            const BOM = "\uFEFF"; // UTF-8 BOM 確保 Excel 顯示中文正常
            const blob = new Blob([BOM + csvContent], {
              type: "text/csv;charset=utf-8;",
            });
            const url = URL.createObjectURL(blob);

            const link = document.createElement("a");
            link.href = url;
            link.setAttribute("download", filename);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);            
    };

    //依據回傳頁數調整回傳進度
    const adjust_progress_time_delay = ( page_allnum) => {  
     
      // let total_num = !isNaN(page_allnum) && Number(page_allnum) > 0 ? Number(page_allnum) : 1;
      // let count  = 1;
      //做級距分別
      // while( total_num > 10)  {
      //     total_num /= 10;
      //     count = count +1;
      // } ;
      // return count;

      const num = Number(page_allnum);
      if (!num || num <= 0) return 1;   
      return Math.floor(Math.log10(num)) + 1;
    }


    // 假進度
    useEffect(() => {
      if (!loading2) return;

      let timeDelay = progess_time_base * adjust_progress_time_delay(totalPages);
      const adjust_timedelay = timeDelay/2;
      // console.log("原來timeDelay= "+ timeDelay)      
      // console.log("調整後timeDelay= "+ adjust_timedelay)

      const timer = setInterval(() => {
        setProgress(prev => {
           if (prev >= 99) return prev;
           const inc = Math.floor(Math.random() * 6) + 5; // 5~10 整數
           return Math.min(prev + inc, 99); // ✔ 重點：累加
        });
      }, adjust_timedelay);

      return () => clearInterval(timer);
    }, [loading2]);



    //當查詢數據持續更新時,提供csv下載
  useEffect(() => {

    console.log("data_excel_info 狀態為="+ typeof data_excel_info +  "資料數據長度為= " + data_excel_info.length );

    if (!Array.isArray(data_excel_info) || data_excel_info.length === 0) 
    {
        toast.error("無電芯數據|no BatteryCell info");
        return;
    }
      
    //嚴謹確認是否有key array 
    const firstRow = data_excel_info?.[0] ?? {};

    const check_Null_keys = Object.keys(firstRow).some(r => (filter_title || []).includes(r));
    const allExist = column_list.every(key => Object.keys(firstRow).includes(key));
    // console.log("第一筆header 內容:"+ Object.keys(data_excel_info[0]) +  " 確認是否有不相關的keys:" + check_Null_keys); 
    //有偵測到無效的key及沒有對應mapping
    if(check_Null_keys|| !allExist)  return;

    console.log("都有對應suliting Keys@");

    const header = Object.keys(firstRow);     
    const url = export_Sulting_ToCSV(
       header,
       data_excel_info,
      "export.csv"
    );

    toast.success("已產出csv");

    setTimeout(() => {
      // setCsvUrl(url);
      if (url) {
          URL.revokeObjectURL(url);
          setCsvUrl(null);
          console.log(`已經清除${url},已取消匯出/釋放資源`);
      }      
    }, 1500);
  },[data_excel_info]);


  return (    
    <div className="quantify_data_graph">         
      {/* switch */}
      <div className="switch_wrapper">
        <header className="title_name_header"> {!isfullCapdata ?"電芯品質管控採樣統計圖":"電芯數據流原始資訊"}</header>
        <input
          type="checkbox"
          id="switch"
          checked={isfullCapdata}
          onChange={handleToggle}
        />
        <label htmlFor="switch">
          <span className="switch-txt">
            {isfullCapdata
              ? "全年月日數據"
              : `電芯序號分容數據`}
          </span>
        </label>
      </div> 
      {!isfullCapdata ? (
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
           <React.Fragment>               
                <div
                  ref={chartRef_3_radio}
                  style={{ width: "100%" ,height: "520px", minHeight: "420px"}}
                />
                </React.Fragment>        
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
      </div> ) : (
        <>
          {/* ---------------- 分頁按鈕 ---------------- */}
          {loading && <p>Loading...</p>}
          <div>
              <label className="serial_label_setting"
                      style={{ width: getprefix_Width(
                    !isprefixLoading ? "電芯前綴" : "電芯前綴切換中..."
                  ) }}
              >                 
                 {!isprefixLoading ? "電芯前綴":"電芯前綴切換中..."} 
              </label> 
              {isprefixLoading ? (
              <div className="loading-wrapper">
                <div className="loading-spinner"></div>
                <span>電芯前綴切換中...</span>
              </div>
              ) : (
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
               )
              }
                           
                {/* <React.Fragment>               
                <div
                  ref={chartRef_3_radio}
                  style={{ width: "100%", height: "520px",minHeight: "420px"}}
                />
                </React.Fragment>  
                */}
                <div className="radio-container">
                    <label style={{ paddingRight: "35px", fontSize: "26px"  , transform: "translate(31%, 1%)"}}>
                      請選擇站別▶{" "}
                    </label>
                    {sulting_options.map((opt) => (
                      <label
                        key={opt.value}
                        className={`radio-label ${
                          selectedOption === opt.value ? "isSelected" : ""}
                          ${opt.value.includes("pf") ? "pfStyle" : ""}
                          ${opt.value.includes("cc") ? "ccStyle" : ""}                          
                          `                          
                        }
                      >
                        <input
                          type="radio"
                          name="options"
                          value={opt.value}
                          checked={selectedOption === opt.value}
                          onChange={() => setSelectedOption(opt.value)}
                        />
                        <span className="label-text">{opt.label}</span>
                      </label>
                    ))}
                </div>
           </div>		
           <div>      
              <InputGroup className="mb-3" style={{marginBlock:"inline-block" , marginTop:"2em"}}>
                    <DropdownButton
                        variant="outline-secondary"
                        title={option_cap || "全部資料"}
                        id="input-group-dropdown-1"
                    >
                        <Dropdown.Item onClick={() => setOption('全部資料')}>全部資料</Dropdown.Item>                      
                        { selectedOption === "sulting_cc" && (
                          <>
                            <Dropdown.Item onClick={() => setOption('CC1分容未分選')}>CC1分容未分選</Dropdown.Item>
                            <Dropdown.Item onClick={() => setOption('CC2分容32分選')}>CC2分容32分選</Dropdown.Item>
                          </>    
                        )}                        
                    </DropdownButton>
                    <Form.Control
                        aria-label="Text input with dropdown button"
                        placeholder= { String(placeholder_cellstr_only.includes(option_cap))?'請輸入欲查詢之電芯號(空預設全查詢)':'請輸入欲查詢電容量'}                        
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') {                                
                                handleSearch(e,e.target.value);
                            }
                        }}
                    />
                    <Button variant="primary" onClick={handleSearch}>
                        搜尋
                    </Button>
              </InputGroup>
                <Form.Group controlId="change_handler" style={{ display: 'flex', alignItems: 'center', marginBottom: '1rem' }}>
                        <div style={{ marginRight: '1rem', display: 'flex', flexDirection: 'row', alignItems: 'center'}}> 
                            <div style={{marginRight: "1rem"}}>起始日期:</div>
                            <DatePicker
                                selected={startDate.toDate()}
                                onChange={(date) => setStartDate(moment(date))}
                                dateFormat="yyyy/MM/dd"
                                className="form-control" 
                                popperPlacement="bottom-start"
                                popperProps={{
                                  strategy: "fixed"
                                }}
                            />
                        </div>
                        <div style={{ marginRight: '1rem', display: 'flex', flexDirection: 'row', alignItems: 'center'}}> 
                            <div style={{marginRight: "1rem"}}>結束日期:</div>
                            <DatePicker
                                selected={endDay.toDate()}
                                onChange={(date) => setEndDay(moment(date))}
                                dateFormat="yyyy/MM/dd"
                                className="form-control"
                                popperPlacement="bottom-start"
                                popperProps={{
                                  strategy: "fixed"
                                }}
                            />
                                           
                        </div>
                        {/* ---------------- 匯出export excel等待動態進度表 ---------------- */}
                            {loading2 &&
                              <div className="progress-box">
                                <div className="progress-bar">
                                  <div className="progress-fill" style={{
                                      width: `${Math.min(progress, 100)}%`,
                                      backgroundColor: getProgressColor(progress)
                                    }} />
                                </div>
                                <div>{progress}%</div>
                              </div>
                            } 
                            {!loading && <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>                            
                                    <Button 
                                        variant="btn btn-outline-primary"                                        
                                        onClick={(e) => exportCSV_NotrueGroess(e)} 
                                        // onClick={(e) => startExport(e)}
                                    >
                                        <i className="bi bi-arrow-left me-1"></i> 轉為csv下載
                                    </Button>                        
                             </div>           
                         }                                                                                                                                                               
                                                         
                    </Form.Group>
            </div>
            <div style={{ maxHeight: 500 , overflow: "auto" , border: "1px solid #ddd"}}>
                <Table  
                      striped
                      bordered
                      hover
                      style={{ textAlign: "center", verticalAlign: "middle" , width: "100%", marginBottom: 0,borderCollapse: "collapse"}}       
                    >	  
                    <thead style={{ 
                                      position: "sticky", 
                                      top: 0, 
                                      backgroundColor: "#f8f9fa",
                              }}>
                            <tr>			
                          {/*取物件內指向label {[]}*/}
                        {Object.values(group_direct_sulting_fields).flatMap( group => {
                           const labelMap = selectedOption === "sulting_cc"?group[0]:
                                            selectedOption === "sulting_pf"?group[1]:
                                             {};                            
                           return Object.entries(labelMap)
                           .filter(([key]) => !filter_title.includes(key))                     
                           .map(([key, label]) => (
                                <th key={key} style={{
                                    minWidth: 120,
                                    backgroundColor:"#FFFF00",
                                    fontWeight: "bold",
                                    // display: "inline-block",
                                 }}>								  
                                    {label || key}						 
                                </th>
                            ));
                        })}
                      </tr>
                  </thead>
                  <tbody>
                    {loading? Array.from({ length: 8 }).map((_, rowIdx) => (
                            <tr key={rowIdx}>
                              {column_list.map((col, colIdx) => (
                                <td key={colIdx}>
                                  <div className="skeleton-cell" 
                                       style={{ width: `${60 + Math.random() * 40}%` }}
                                  ></div>
                                </td>
                              ))}
                            </tr>
                          ))
                        : data_info.map((row, idx) => (
                            <tr key={row.id + '-' + idx}>
                                {column_list.map(col => (
                                    <td key={col} style={{ 
                                         minWidth: "150px", 
                                         maxWidth: "fit-content", 
                                         width: "auto", padding: "8px",
                                        textAlign: "center",
                                        whiteSpace: "nowrap",
                                        overflow: "hidden",
                                        textOverflow: "ellipsis",
                                    }}>
                                        {row[col] !== null ? row[col] : ''}                                        
                                    </td>
                                ))}
                            </tr>
                        ))}                                                
                  </tbody>	  
                </Table>
            </div>
            {/* <div style={{ marginTop: 20 }}> */}
            <div className="flex justify-center items-center gap-4 mt-6 flex-wrap">
                <button onClick={prevPage} disabled={page === 1}>
                  ◀ Prev|上一頁
                </button>
                <span style={{ margin: "0 10px" }}>
                  目前頁數Page {page >totalPages?totalPages:page} / {totalPages}
                </span>
                <button onClick={nextPage} disabled={page === totalPages}>
                  Next|下一頁 ▶
                </button>
                <input
                  type="number"
                  placeholder="頁數"
                  value={inputPage}
                  onChange={(e) => setInputPage(e.target.value)}
                  className="w-8 px-1 py-1 border rounded"
                  onKeyDown={(e) =>{
                     if (e.key !== "Enter") return; 

                     e.preventDefault(); //  防止 form / input 重送
                     goToPage(inputPage);                     
                  }}
                />
                    <button
                      onClick={() => goToPage(inputPage)}
                      className="px-3 py-1 bg-green-350 text-black rounded"
                    >
                      跳頁
                  </button>                               
              </div>
        </>
      )
      }
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
  );

}



export default Quantify_data_graph;

