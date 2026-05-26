import React, { useState, useEffect, useCallback, useMemo ,useRef  } from "react";
import debounce from "lodash.debounce";
import config from "../config";
import { Form, Modal , Button, Toast } from "react-bootstrap";
import dayjs from "dayjs";
import Table from "react-bootstrap/Table";
import { Container, Row, Col } from "react-bootstrap";
import "./repair_popform.scss";
import axios from "axios";
import moment from "moment";
//成功提示套件
import { toast } from "react-toastify";
import * as echarts from "echarts/core";
import { BarChart , LineChart, PieChart } from "echarts/charts";
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  LegendComponent,
  LabelLayout,
  UniversalTransition
} from "echarts/components";
import { SVGRenderer, CanvasRenderer } from "echarts/renderers";


echarts.use([
  LineChart,       
  PieChart,         
  BarChart,
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  LegendComponent,
  CanvasRenderer,
]);


//配方組態物件props 傳遞渲染
const ProportionalAnalysis = ({ show, onHide , RecipeDataList , Delte_Ack ,centered}) => { 
  const chartRef_S = useRef(null); // 创建 ref 来引用 DOM 元素
  const chartRef_Details = useRef(null); // 创建 ref2 来引用 DOM 元素
  const chartRef_ver = useRef(null);  //指參照一次ref
  const chartRef = useRef(null);  //指參照一次ref
  const [visibleRows, setVisibleRows] = useState(20); // Initially show 20 rows
  const [formulasource, setFormulaSource] = useState([]);
  const [prescipt_data, setPrescipt_data] = useState([]);
  const [ver_currentsource, setCurrentVerSource] = useState([]);
  const [haveDelete_ver, sethaveDelete_ver] = useState([]);
  const [title_ver_status, settitle_ver_status] = useState("");
  const [activeDimension, setActiveDimension] = useState(1);
  const [grouped_recipe, setGrouped_Recipe] = useState({
    正極混漿: [],
    負極混漿: [],   
  }); // 分組資料

  const { id, mainform_code, station} = RecipeDataList;
  const isdel_mode = Delte_Ack;
  const workid = RecipeDataList.id;
  const RecipeMaincode = RecipeDataList.mainform_code;
  const side_station = RecipeDataList.station;
  const version_str = RecipeDataList.control_version;
  
  let lockedDimension = null; // 鎖定版本 flag 加入參照
  
  const mixing_name = side_station.includes("MixingCathode")?"正極混漿":"負極混漿";

  
  console.log("目前ProportionalAnalysis已收到結構為" +JSON.stringify(RecipeDataList,null,2));


  const get_common_units = (unit ,val) =>{
    let caculator_count = parseFloat(val);
    //若是公斤單位,則需要轉換公克
    if(String(unit).includes("kg")){
          caculator_count *= 1000;
    }       
    return (caculator_count).toFixed(2)??0.00;    
  }

   const getAxisIndex = (event) => {
      const axis = event?.axesInfo?.[0];
       if (!axis) return null;

      // ⭐ 正確來源是 valueIndex
      if (typeof axis.valueIndex === "number") return axis.valueIndex;

      // fallback（某些版本）
      if (typeof axis.value === "number") return axis.value;

      return null;
  };


  const buildPieDataFromRow = (source, rowIndex) => {
     const idx = Number(rowIndex);

      if (!Number.isFinite(idx)) return [];

      const row = source[idx + 1]; // ⭐ header offset
      if (!row) return [];

      const header = source[0];

      return header.slice(1).map((col, i) => ({
        name: col,
        value: Number(row[i + 1] ?? 0),
      }));
  };

  //手動轉換Pie 結構材料分配圖比例
  const buildPieData = (source, rowIndex) => {
     const row = source[rowIndex+1]; 

     console.log("buildPieData row 目前為:" + row);
      if (!row) return [];

      const header = source[0];

      return header.slice(1).map((col, i) => ({
        name: col,
        value: row[i + 1],
      }));   
  };

//確認RecipeDataList配方結構 ,並將相關站點版本資訊內容init載入後續渲染使用
useEffect(() => {  

   let ignore = false;
    console.log("當前版本 PK 序號 = " + Number(workid) , "Delte_Ack 動作與否:" +  isdel_mode);

    const search_presponsecription_related = async () => {
      try {
            const response = await axios.get(
               `${config.apiBaseUrl}/mixprocess/recipe_mixing_classinfo`,
              // "http://localhost:3009/mixprocess/recipe_mixing_classinfo",
            {
                params: {                             
                  recipe_code: RecipeMaincode,                   
                  side_station: side_station
                }
            });

          if (ignore) return;

          const result =  response.data;

          console.log("目前接收search_presponsecription_related api 回傳清單為= " + JSON.stringify(result,null,2)); 
      
          
          if (response.status === 200 && Object.values(result.allinfo_data).length > 0)
          {

            const data_formula = Object.values(result.allinfo_data);

            let viewisDelteRaw=[];

            // 1️先收集所有 itemname（當欄位）
            const allItems = [
              ...new Set(
                data_formula.flatMap(row =>
                (row.datainfo||[])
                  .filter(d => d?.itemname)
                  .map(d => d.itemname.trim())
                )
              )
            ];
            
            const header_allsource = [
              ["ver版本", "流水號",...allItems]
            ];

            //  console.log("項目header 整理為:"+ header_allsource);


            //走訪每個allItems 指定item 的提交單位量
            data_formula.forEach(row => {
              
              console.log("data_formula 回傳RAW data 結構為: "+ JSON.stringify(row,null,2));
              
              // 判定有刪除紀錄isdelete === 1情況下
              if (isdel_mode && row.isdelete) {
                //sethaveDelete_ver([]);
                  //判定若是刪除模式查詢在閃過原先Pk_id,不納入參考
                  if(row.pkid === Number(workid)){
                    console.log("當前檢視NG (workid) = "+ Number(workid));
                    console.log("有閃過->" + row.pkid  + "  isdel_mode 模式狀態為: " +isdel_mode);
                    // viewisDelteRaw.push(row);
                    sethaveDelete_ver(prev => [...prev, row]);
                  }
               
                return;
              }else {
                  if( !isdel_mode && row.isdelete )
                  {
                      console.log("有閃過->" + row.isdelete  + "  isdel_mode 模式狀態為: " +isdel_mode);
                      return;
                  }
              }

              const data_qty ={};

              row.datainfo.forEach(d => {
                data_qty[d.itemname] = Number(get_common_units(d.unit,d.qty));
              });

               const rowData = [
                row.version,
                row.pkid,
                ...allItems.map(name => data_qty[name] ?? 0) // 沒有就補 0
              ];

              header_allsource.push(rowData);
            });

            // console.log("被刪除的Raw = " + JSON.stringify(viewisDelteRaw,null,2));
                    
             //找出當前登入版本之序號(刪除模式則找PK ID , 其他模式找ver號碼)-----start--------
             const idx = header_allsource
                        .slice(1)
                        .findIndex( row => {   
                                      return isdel_mode
                                      ? row[1] === Number(workid)
                                      : row[0] === version_str;
                                    });

          
             const get_currentver_raw =  idx != -1
                        ? buildPieData(header_allsource, idx)
                        : [];

            const get_IgoneKPID_currentver_raw = get_currentver_raw.filter(row => !row.name.includes("流水號"));
            
             setCurrentVerSource(get_IgoneKPID_currentver_raw);
             //------------------------------當前登入版本之序號 end----------------------------------------------

              //  console.log("項目header_allsource 總整理為:"+ header_allsource  + "\r\n"+" 總共筆數:"  + Object.values(header_allsource).length );
             console.log("header_allsource e-chart渲染組態為:"+ JSON.stringify(header_allsource,null,2));

             setFormulaSource(header_allsource);

             //敘述當前版本狀態
             const descript_status = isdel_mode? `已刪除紀錄序號:${workid}`:`在線中:版本號:${version_str}`;
             settitle_ver_status(descript_status);
             
              // console.log(" get_currentver_raw 當前版本置入pie 數據:"+ JSON.stringify(get_currentver_raw,null,2));  

            //  const get_currentver_raw = header_allsource.slice(1).flatMap((row, idx) => {           
            //     // console.log(`第${idx+1}個->數據組value = `+ row);               
            //     if (row[0] === version_str) {
            //         console.log(`匹配到第${idx}個`, row);
            //        // return buildPieData(header_allsource, idx);
            //     }                   
            //  })

            
     
          }

      } catch (error) {
          console.error("Error fetching options:", error);
          return "";
      }
   };
   
   search_presponsecription_related();

  return () => {
    ignore = true;
  };

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /*之前不考慮state 直接 setOption*/
  // useEffect(() => {     
     
  //   if (!Array.isArray(formulasource) || formulasource.length === 0 || !chartRef_Details.current) {
  //   return;
  //  }

  //   const dom = chartRef_Details.current;
  //   // ⭐ 永遠清掉舊 instance
  //   let myChart = echarts.getInstanceByDom(dom);

  //   if (myChart) {
  //     myChart.dispose();
  //   }

  //   myChart = echarts.init(chartRef_Details.current, "dark", {
  //     renderer: "canvas",
  //     useDirtyRect: false,
  //   });

  //   //將既有serial 重整帶入選染區域設定成像,header[0] 不納入serial 

  //   try {
  //   // const mockData = [
  //   //   ["ver版本", "A", "B", "C"],
  //   //   ["v1", 10, 20, 30],
  //   //   ["v2", 15, 25, 35],
  //   //   ["v3", 20, 30, 40],
  //   // ];
    
  //   const header = formulasource[0];

  //   const lineSeries = header.slice(1).map((material, index) => ({   
  //     name: material, // v1.0 / v2.0
  //     type: "line",
  //     smooth: true,
  //     seriesLayoutBy: "column",
  //     encode: {x: 0,y: index + 1},
  //     emphasis: { focus: "series" },
  //   })); 

  //   const defaultDim = 1;

  //   console.log("lineSeries = "+ JSON.stringify(lineSeries,null,2));
   
  //      // 初始化图表
  //     // myChart = echarts.init(chartRef_Details.current, "dark", {
  //     //   renderer: "canvas",
  //     //   useDirtyRect: false,
  //     // });
      
  // 	  const
	//     option = {
  //       legend: {top:10},
  //       tooltip: {
  //         trigger: "axis",          
  //       },
  //       dataset: {		
	// 	     source: formulasource,         
  //       },				
	//       xAxis: { type: "category" },
  //       yAxis: { type : "value"},
  //       grid: { 
  //         top: "15%", // 調整 grid 以免被 legend 遮住
  //         bottom: "10%",
  //         containLabel: true 
  //       },
  //       //pie 只保留「動態 encode」
  //       series: [
  //           ...lineSeries,
  //           { 
  //             type: "pie", 
  //             id: "pie", 
  //             radius: "30%",
  //             center: ["50%", "25%"],
  //             emphasis: { focus: "self", },               
  //             encode: {
  //               itemName: 0,
  //               value: 1,
  //               tooltip: 1,
  //             }, 
  //             label: {
  //                  formatter: "{b}: {@[1]} ({d}%)",
  //             },            
  //           }
  //       ]
  //     };
            
  //     myChart.setOption(option);

  //     // 1. 延遲 resize 確保 Flexbox 已經穩定
  //     setTimeout(() => {
  //       myChart.resize();
  //     }, 200); 

  //     // 2. 檢查 Canvas 是否真的存在
  //     const canvas = dom.querySelector('canvas');
  //     console.log("Canvas 元素是否存在:", !!canvas);
  //     if (canvas) {
  //       console.log("Canvas 實際渲染寬高:", canvas.width, canvas.height);
  //     }
                
  //     const resizeHandler = () => {
  //       myChart?.resize();
  //     };

  //      window.addEventListener("resize", resizeHandler);

  //     // ⭐ 避免事件重複綁定
  //     myChart.off("updateAxisPointer");
  //     myChart.off("legendselectchanged");


  //     myChart.on("updateAxisPointer", function (event) {
  //       if (lockedDimension !== null) return;
        
  //       const xAxisInfo = event.axesInfo?.[0];
  //       if(!xAxisInfo) return;
  //       const dimension = xAxisInfo.valueIndex + 1;
  //       myChart.setOption({
  //           series: [
  //             {
  //               id: "pie",
  //               encode: {
  //                 itemName: 0,
  //                 value: dimension,
  //                 tooltip: dimension,
  //               },               
  //               label: {
  //                 formatter: "{b}: {@[" + dimension + "]} ({d}%)",
  //               },                
  //             },
  //           ],
  //         });
  //       }
  //     );

  //     myChart.on("legendselectchanged", function (event) {
  //         const selected = Object.entries(event.selected)
  //         .filter(([_, v]) => v)
  //         .map(([k]) => k);

  //          if (selected.length !== 1) {
  //           lockedDimension = null;
  //           return;
  //         }

  //         const selectedName = selected[0];

  //         const colIndex = formulasource[0].findIndex(col => col === selectedName);
 
  //         if (colIndex !== -1) {
  //           lockedDimension = colIndex ;

  //           myChart.setOption({
  //             series: [
  //               {
  //                 id: "pie",                 
  //                 encode: {
  //                   value: colIndex,
  //                   tooltip: colIndex,
  //                 },
  //               },
  //             ],
  //           });
  //         }
  //       });

  //     //async function 裡 return cleanup
  //     return () => {
  //       window.removeEventListener("resize", resizeHandler);
  //       myChart?.dispose();
  //     };

  //   } catch (error) {
  //     console.error("取得資料錯誤", error);
  //   }        
  // }, [ formulasource]);

  const versionMap = useMemo(() => {
    const map = new Map();

    prescipt_data.slice(1).forEach((row, i) => {
      map.set(row[0], i + 1); // v1.0 -> row index
    });

    return map;
  }, [prescipt_data]);

  useEffect(() => {
  if (!Array.isArray(formulasource) || formulasource.length === 0) return;

  console.log("formulasource → prescipt_data");

   //這邊需要忽略流水號 index 位置 ,目前為 1
   const ignoreIndex = 1; // 流水號
   const ignorepkid_Header = formulasource[0].filter((_, i) => i !== ignoreIndex);

   const filterpkid_Data = formulasource.slice(1).map(row =>
      row.filter((_, i) => i !== ignoreIndex)
   );


   const final_Dataset = [ignorepkid_Header, ...filterpkid_Data];

   //將其回傳資料存入usestate 
   setPrescipt_data(final_Dataset);
  
}, [formulasource]);


 const All_Option = useMemo(() => {
      
      if (!Array.isArray(prescipt_data) || prescipt_data.length === 0) {
          return null;
      }

      const ignorepkid_data  = prescipt_data[0];
     
      //最後渲染標頭,內容line
      const lineSeries = ignorepkid_data.slice(1).map((material, index) => ({   
        name: material, // v1.0 / v2.0
        type: "line",
        smooth: true,
        seriesLayoutBy: "column",
        encode: {x: 0,y: index + 1},
        // emphasis: { focus: "series" },
      })); 

     
      console.log("lineSeries = "+ JSON.stringify(lineSeries,null,2));
      
      const DEFAULT_DIM = 0;//預設第一筆材料row 

      return {
            dataset: { source: prescipt_data },
            legend: {},
            tooltip: {
              trigger: "axis",
              axisPointer:{
                type: "line",
                link: [{ xAxisIndex: "all" }],
                triggerTooltip: true,
              },  
              textStyle: {
                fontSize: 20,    
                fontWeight: "bold"
              }    
                // appendToBody: true,
                // renderMode: "html"
            },
            xAxis: { 
               type: "category" ,
               boundaryGap: false,   // ⭐ 讓 pointer 精準對齊
               axisLabel: {
                interval: 0,              // 全顯示
                rotate: 10,               // 避免擠在一起
              },
              name: "版本",
              nameLocation: "middle",
              nameGap: 30,
            },
            yAxis: {
              type: "value",
              name: "公克(g)",
              nameLocation: "end",
              nameTextStyle: {
                padding: [0, 0, 0, 10],
              },              
              minInterval: 100,
              splitNumber: 10
            },
            series: [
              // LINE（固定）
              ...lineSeries,
              // PIE（唯一動態）
              {
                id: "pie",
                type: "pie",
                //datasetIndex: 0, //加 datasetIndex（穩定 pie）
                radius: "30%",  // 大小控制                  
                center: ["52%", "35%"],
                // encode: {
                //    itemName: 0,                       
                //    value: DEFAULT_DIM,
                //    tooltip: DEFAULT_DIM,
                // },
                grid: {
                  containLabel: true
                },                  
                data: buildPieData(prescipt_data,DEFAULT_DIM),
                label: {
                    formatter: `{b}: {@[c]} (g) 占比:({d}%)`,
                    show: true,
                    fontSize: 18,
                    fontWeight: "bold",
                    color: "#fff",   // 深色背景建議

                  // formatter:(params) => {
                  //   console.log("總共參數param= "+ JSON.stringify(params,null,2))
                  //   return `${params.name}: ${params.value} (${params.percent}%)`;
                  // }                  
                },
                labelLine: {
                  length: 10,
                  length2: 8,
                }
              },
            ],
          };
  }, [prescipt_data]);


  useEffect(() => {
    if (!Array.isArray(formulasource) || formulasource.length === 0) return;

    console.log("有正常收到回傳formulasource" + formulasource);

    const dom = chartRef_Details.current;
    if (!dom) return;

    const chart = echarts.init(dom, "dark");

    chartRef.current = chart;

    chart.setOption(All_Option);

    return () => chart.dispose();
  }, [All_Option]);

  useEffect(() => {
    const dom = chartRef_Details.current;
    if (!dom) return;

    // 防止重複 instance
    if (chartRef.current) {
      chartRef.current.dispose();
    }

    let  myChart = echarts.init(dom, "dark", {
      renderer: "canvas",
      useDirtyRect: false,
    });

    chartRef.current = myChart;


    const header = formulasource?.[0] || [];
    
    setTimeout(() => {
      myChart.resize();
    }, 200); 

    // resize handler（只定義一次）
    const resizeHandler = () => {
          myChart.resize();
    };

    window.addEventListener("resize", resizeHandler);

    return () => {
      window.removeEventListener("resize", resizeHandler);
      myChart.dispose();
      chartRef.current = null;
    };
  }, []);


  //局部 update
  useEffect(() => {
    const chart = chartRef.current;
    if (!chart) return;

    const handler = (event) => {
      // const axis = event.axesInfo?.[0];
      // console.log("axis  handle 觸發為 = "+ JSON.stringify(axis,null,2));
     
      const dim = getAxisIndex(event);
      //防止Over index 導致crash
      // if (dim >= prescipt_data.length) return;

      // console.log("hover dim =", dim);
      // console.log("data length =", prescipt_data.length);

      const data = buildPieData(prescipt_data, Number(dim));

      // console.log("data最後pie =", data);


      const colIndex = versionMap.get(dim);
      // console.log("colIndex versionMap算出為: "+colIndex);
      // if (colIndex == null) return;


      chart.setOption(
        {
          series: [
            {
              id: "pie",
              data,             
              // encode: {
              //   itemName: 0,
              //   value: dim,
              // },
            },
          ]
        },
        false // ⭐ 關鍵：禁止重建
      );
    };

    chart.on("updateAxisPointer", handler);

    return () => {
      chart.off("updateAxisPointer", handler);
    };
  }, [prescipt_data]);


  //legend click事件綁定設置
  useEffect(() => {
      const chart = chartRef.current;
      if (!chart) return;

       const handler = (event) => {
          const selected = Object.keys(event.selected).filter(
            (k) => event.selected[k]
          );

          if (selected.length !== 1) return;

          const header = prescipt_data?.[0];

          //需要array 型態
          if(!Array.isArray(header)) return;

          const colIndex = header.findIndex((c) => c === selected[0] );
          
          if (colIndex < 1 ) return; // 避免抓到第一欄

           // 找最大值的版本（row）
          // let maxRowIndex = 1;
          // let maxVal = -Infinity;

          // for (let i = 1; i < prescipt_data.length; i++) {
          //   const val = prescipt_data[i][colIndex];
          //   if (val > maxVal) {
          //     maxVal = val;
          //     maxRowIndex = i;
          //   }
          // }

          chart.setOption(
            {
              series: [
                {
                  id: "pie",
                  data: buildPieData(prescipt_data, colIndex),
                  // encode: {
                  //   itemName: 0,
                  //   value: colIndex,
                  //   tooltip: colIndex,
                  // },
                },
              ],
            },
            false // ⭐ 關鍵：不重建
          );
        };
      
      chart.on("legendselectchanged", handler);

      return () => {
        chart.off("legendselectchanged", handler);
      };

  }, [prescipt_data]);


  //useEffect（只負責 setOption）
  useEffect(() => {
    if (!chartRef.current || !All_Option) return;

    chartRef.current.setOption(All_Option, {
      notMerge: false,
      lazyUpdate: true,
    });

    requestAnimationFrame(() => {
      chartRef.current?.resize();
    });
  }, [All_Option]);

  //當前版本數據流轉換Pie圖觸發流程
  useEffect(() => {  
    // console.log("當前版本數據流為 = "+ JSON.stringify(ver_currentsource,null,2));

    const dom = chartRef_S.current;
    if (!dom) return;

    // 防止重複 instance
    if (chartRef_ver.current) {
      chartRef_ver.current.dispose();
    }

	 let myChart_single;
   let option;

   try {	 
		 // 初始化图表
		 myChart_single = echarts.init(dom, {
		  renderer: "canvas",
		  useDirtyRect: false,
		});

		chartRef_ver.current = myChart_single;
		
		option = {
			legend: {},
      toolbox: {  
        show: true,  
        top: 10, 
        right: 10,    
       
        iconStyle: {
          borderColor: "#449da0"   // ⭐ 白色 icon（dark theme 必加）
        },

        feature: {
          saveAsImage: {
            show: true,
            title: "下載圖片",
            type: "png",          // png / jpeg
            name: "fdg", // 檔名
            pixelRatio: 50         // ⭐ 提高清晰度（很重要）
          }
        }
      },
			tooltip: {
			   trigger: 'item',
         formatter: '{a} <br/>{b}: {c} ({d}%)',         
			   showContent: false,
         textStyle: {
              fontSize: 10,    
              fontWeight: "bold"
         }    
			},		 
			xAxis: { type: "category" },
			yAxis: { gridIndex: 0 },
			grid: { top: "65%" },     
			series: [
			   {
				  name: `Recipe 配方量(比重)`,
				  type: 'pie',
				  radius: ['35%', '70%'],
				  labelLine: {
					  length: 30
				  },
				  label: {
            formatter: (params) => {          
               const ser_name = params.seriesName;
               const name = params.name;
               const value = params.value;
               const percent = params.percent;
               
                // console.log("配方項目name = "+ name)
                // console.log("總共params = "+ JSON.stringify(params,null,2))
                const formattedName =
                name.length > 10
                  // ? name.slice(0, 16) + "\n" + name.slice(16)
                  ?name.slice(0, 10) + "..."
                  : name;
                  
              return `{a|${ser_name}}{abg|}\n{hr|}\n  {b|${formattedName}：}{cval|${value}(g)} \n {per|${percent}%}`;
            },            
            backgroundColor: '#F6F8FC',            
            borderColor: '#8C8D8E',
            borderWidth: 3,
            borderRadius: 6,
            padding: [10, 14, 10, 14],                        
            rich: {
              a: {
              color: '#090b11',
              lineHeight: 32,
              align: 'center'
              },
              hr: {
              borderColor: '#8C8D8E',
              width: '100%',      
              borderWidth: 3,             
              height: 0
              },
              b: {
              color: '#111113',
              fontSize: 15,
              fontWeight: 'bold',
              lineHeight: 33
              },            
              cval: {
                fontSize: 18,        
                fontWeight: 'bold',
                color: '#000'
              },
              per: {
                color: '#fff',
                fontSize: 13,
                backgroundColor: '#2861d3',
                padding: [3, 4],
                borderRadius: 7
              }
					 }
				 },
				//   data: [
				// 		{ value: 1048, name: 'Baidu' },
				// 		{ value: 335, name: 'Direct' },
				// 		{ value: 310, name: 'Email' },
				// 		{ value: 251, name: 'Google' },
				// 		{ value: 234, name: 'Union Ads' },
				// 		{ value: 147, name: 'Bing' },
				// 		{ value: 135, name: 'Video Ads' },
				// 		{ value: 102, name: 'Others' }      
				//  ],
        data: ver_currentsource
		   },      
		 ]
    };

 
		 
	    myChart_single.setOption(option, { notMerge: true });

      const resizeHandler = () => {
          myChart_single.resize();
      };
		  
	    window.addEventListener("resize", myChart_single.resize());

      return () => {
        window.removeEventListener("resize", resizeHandler);
        myChart_single.dispose();
        chartRef_ver.current = null;
      };
   } catch (error) {
      console.error("取得資料錯誤", error);
   }

  }, [ver_currentsource]);



  //解析設備名稱與ID
  const parseDevice = (item) => {
    const [id, ...nameParts] = item.split("-");

    return {
      id,
      name: nameParts.join("-"),
    };
  };

  const toNumber = (str) => parseInt(str.replace("M", ""), 10);

  const isPositiveInteger = (value) => {
    const num = Number(value);
    return Number.isInteger(num) && num > 0;
  };

  const isLarge_compare_ID = (ST_ID, ED_ID) => {
    const start_ID = Number(ST_ID);
    const end_ID = Number(ED_ID);
    // console.log("比較大小 Start_ID = " + start_ID + " End_ID =  " + end_ID);
    return end_ID >= start_ID;
  };

  useEffect(() => {
    //setVisibleRows(20); // Reset visible rows when modal opens
   
    const work_mixing_select = Object.entries(grouped_recipe).forEach(([key, value],index) => {
      if( key.includes(mixing_name)) {value = RecipeMaincode} ;
      return value;
    });

  }, [grouped_recipe]);

    console.log(
      "grouped_recipe changed:",
      JSON.stringify(grouped_recipe, null, 2)
    );

  const chunkArray = (array, size) => {
    const chunks = [];
    for (let i = 0; i < array.length; i += size) {
      chunks.push(array.slice(i, i + size));
    }
    return chunks;
  };


  	useEffect(() => {  
		
		console.log("刪除版本RAWHHight= "+ JSON.stringify(haveDelete_ver,null,2));
	
	 }, [haveDelete_ver]);


  const HorizontalDeviceTable = ({ devices, groupSize = 3 }) => {
    const rows = chunkArray(devices, groupSize); // 每行 groupSize 組

    return (
      <Table
        striped
        bordered
        hover
        style={{
          textAlign: "center",
          verticalAlign: "middle",
          width: "100%",
          tableLayout: "fixed",
        }}
      >
        <thead>
          <tr>
            {Array.from({ length: groupSize }).map((_, i) => (
              <React.Fragment key={i}>
                <th>財產編號</th>
                <th>設備名稱</th>
              </React.Fragment>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((rowGroup, rowIndex) => (
            <tr key={rowIndex}>
              {rowGroup.map((device, i) => (
                <React.Fragment key={i}>
                  <td>{device.id}</td>
                  <td>{device.name}</td>
                </React.Fragment>
              ))}
              {/* 補足不足的欄位（如果最後一行不足 groupSize） */}
              {Array.from({ length: groupSize - rowGroup.length }).map(
                (_, i) => (
                  <React.Fragment key={`empty-${i}`}>
                    <td></td>
                    <td></td>
                  </React.Fragment>
                )
              )}
            </tr>
          ))}
        </tbody>
      </Table>
    );
  };

  const handleScroll = (e) => {
    // console.log(
    //   e.target.scrollHeight,
    //   e.target.scrollTop,
    //   e.target.clientHeight
    // );

    const bottom =
      e.target.scrollTop + 5 + e.target.clientHeight >= e.target.scrollHeight;

    if (bottom) {
      console.log("Reached the bottom");
      setVisibleRows((prevVisibleRows) => prevVisibleRows + 20); // Load next 20 rows
    }
  };

  const handleCancel = () => {
    

    // 重置表單（假設你有使用 useRef 取得 form DOM）
    // formRef.current?.reset();
     // 清理圖表實例
    if (chartRef_S.current) {
      try {
        const chartInstance = echarts.getInstanceByDom(chartRef_S.current);
        if (chartInstance && !chartInstance.isDisposed()) {
          chartInstance.dispose();
        }
      } catch (error) {
        console.warn("清理單一圖表時發生錯誤:", error);
      }
    }

    if (chartRef_Details.current) {
      try {
        const chartInstance = echarts.getInstanceByDom(chartRef_Details.current);
        if (chartInstance && !chartInstance.isDisposed()) {
          chartInstance.dispose();
        }
      } catch (error) {
        console.warn("組件卸載時清理多數據版本圖表發生錯誤:", error);
      }
    }

    chartRef_S.current = null;
    chartRef_Details.current = null;
    chartRef.current = null;
    chartRef_ver.current = null;

    // console.log("handleBack 清理完成 - 調用 onHide");
    // 讓 React-Bootstrap 處理所有 Modal 狀態管理
    onHide();
  };

  const handleSave_Recipe_Picture = (e) => {
    e.preventDefault();

    const chart = chartRef_ver.current;

    if (!chart || typeof chart.getDataURL !== "function") {
      console.log("狀態 typeof chart.getDataURL = "+typeof chart.getDataURL);
      console.log(" chart 目前為: "+ chart);
      console.error("chart 尚未初始化或不是 echarts instance");
      return;
    }

    const url = chart.getDataURL({
      type: "png",
      pixelRatio: 2,
      backgroundColor: "#FFF"
    });

    const a = document.createElement("a");
    a.href = url;
    a.download = `${mixing_name}-配方編號:${RecipeMaincode}-版本${version_str}.png`;
    a.click();
  };


  // Handle form submission
  const handleSubmit = (e) => {
    e.preventDefault();

    
  };

  return (
    <div className="popform_display">
      <form onSubmit={handleSubmit}>
        <div className="modal-overlay">
          <div className="modal-container">
            <div style={{ display: "flex", justifyContent: "center" }}>
              <label
                style={{
                  fontWeight: "bold",
                  fontStyle: "normal",
                  paddingRight: "180px",
                  paddingTop: "20px",
                  fontSize: "41px",
                  // marginInline: "10px",
                }}
              ></label>

              <h3
                style={{
                  // justifycontent: "center",
                  // marginRight: "120px",
                  // paddingTop: "25px",
                  // fontSize: "45px",
                  // fontfamily: "Bungee Spice",
                  // color: "#FFFF00",
                  // backgroundColor: "#0000E3	",
                  marginTop: "15px",
                  marginRight: "120px",
                  paddingTop: "25px",
                  paddingLeft: "12px",
                  fontSize: "41px",
                  fontFamily: "Bungee Spice", // ← 注意這裡要大寫 F
                  color: "#FFFF00",
                  backgroundColor: "#0000E3",
                }}
              >
                {mixing_name}-配方編號:{RecipeMaincode}-{title_ver_status}-組合比例數據圖表
              </h3>
              <br />

              <button
                type="button"
                style={{ marginLeft: "70px", backgroundColor: "red" }}
                onClick={handleCancel}
              >
                關閉
              </button>
              <button
               style={{ marginLeft: "20px", backgroundColor: "#7edf46" }}
               onClick={handleSave_Recipe_Picture}>下載圖片</button>
            </div>
            <div
              style={{
                height: "80vh",        // 用視窗高度偵測            
                overflowY: "auto",                
                display: "inline-block",
                width: "95vw",               
                //paddingBottom: "1150px", // ✅ 給所有內容一個統一的底部空間
              }}
            >
                 {isdel_mode && haveDelete_ver.map((raw, rawIndex) => (
                            <div key={rawIndex} style={{ marginTop: "20px" }}>

                              <h3>
                                刪除版本: {raw.version} / 資料庫序號: {raw.pkid}
                              </h3>

                              <table
                                border="1"
                                cellPadding="8"
                                style={{
                                  borderCollapse: "collapse",
                                  width: "100%",
                                  background: "#dff5f5",
                                  color: "#0a0303",
                                  fontSize:"20px"
                                }}
                              >
                                <thead>
                                  <tr>
                                    <th style={{ border: "1px solid #000", padding: "8px" }}>材料名稱</th>
                                    <th style={{ border: "1px solid #000", padding: "8px" }}>數值</th>
                                    <th style={{ border: "1px solid #000", padding: "8px" }}>單位</th>
                                    <th style={{ border: "1px solid #000", padding: "8px" }}>原料固含量</th>
                                    <th style={{ border: "1px solid #000", padding: "8px" }}>設計比例</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {raw.datainfo.map((item, idx) => (
                                    <tr key={idx}>
                                      <td>{item.itemname}</td>
                                      <td>{item.qty}</td>
                                      <td>{item.unit}</td>
                                      <td>{item.spec}%</td>
                                      <td>{item.ingredradio}%</td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                 ))}
                <div className="d-grid p-20">
                    <div className="text-center mb-4" style={{fontSize:"30px" ,marginRight:"1050px" ,marginTop:"50px"}}>配方比例配置(%)</div>

                    {/* 🔥 關鍵：左右排版 */}
                  <div
                    style={{
                      display: "flex",
                      gap: "16px",
                      width: "100%",
                    }}
                  >
                    <div style={{ width: "20%" , flex: 1 , marginRight:"10px" }}>
                       <div ref={chartRef_S} style={{ width: "110%", height: "700px"  }}>
                         
                       </div>
                    </div>  
                    <div style={{ width: "70%" , flex: 1}}>                                           
                    <div className="text-center mb-4" style={{fontSize:"30px" ,marginRight:"110px"}}>各版本配方比例配置(%)</div>
                      <div
                          ref={chartRef_Details}
                          style={{ width: "100%", height: "900px"  }}
                      >                    
                      </div>
                    </div> 
                   </div>
                </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  );
};

export default ProportionalAnalysis;
