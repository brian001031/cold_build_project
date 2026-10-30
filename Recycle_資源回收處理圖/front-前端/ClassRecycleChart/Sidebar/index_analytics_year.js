import React, { useState, useEffect, useRef } from "react";
import Form from "react-bootstrap/Form";
import config from "../../../config";
import axios from "axios";
import * as echarts from "echarts/core";
import {
  DatasetComponent,
  GraphicComponent,
  GridComponent,
} from "echarts/components";
import { BarChart } from "echarts/charts";
import { SVGRenderer, CanvasRenderer } from "echarts/renderers";
import {
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from "react-chartjs-2";


echarts.use([SVGRenderer, CanvasRenderer]);

echarts.use([
  DatasetComponent,
  GraphicComponent,
  GridComponent,
  BarChart,
  CanvasRenderer,
]);

let option;
let yearsdata = [];
let startIndex = 10;

//舊式欄位
const recycle_category_name_old = [
  "廢塑膠混合物",
  "廢木材棧板",
  "非有害油泥",
  "金屬廢料混合物(熱處理)",
  "金屬廢料混合物(物理)",
  "底料NMP",
  "E004NMP(回收)",
  "含鋁混和五金廢料(卷料)",
  "含鋁混和五金廢料(邊/片料)",
  "含銅混和五金廢料(卷料)",
  "含銅混和五金廢料(邊/片料)",
  "廢電子零組件",
  "廢塑膠(紙箱含塑膠混和物)",
  "廢塑膠(鋁塑膜)",
  "廢塑膠(PP膜)",
  "廢銅",
  "廢鋁",
  "廢乾電池",
];

//新式欄位
const recycle_category_name = [
  "廢塑膠混合物",
  "廢木材棧板",
  "非有害油泥",
  "金屬廢料混合物((熱處理/物理)",
  "回收NMP/NMP (黑)",
  "含鋁混和五金廢料(卷料/邊片料)",
  "含銅混和五金廢料(卷料/邊片料)",
  "廢電子零組件",
  "廢塑膠(紙箱含塑膠混和物)",
  "廢塑膠(鋁塑膜/PP)",
  "廢銅",
  "廢鋁",
  "廢乾電池",
];

const keyNameMap = {
  totalamont: "總年度量加總(kg)統計",
  total_negative_amont: "總年度-負值(kg)統計",
  total_positive_amont: "總年度+正值(kg)統計",
};

const updateFrequency = 3000;
const dimension = 0; // 使用第 0 欄位的數值來畫圖
const countryColors = {
  Australia: "#00008b",
  Canada: "#f00",
  China: "#ffde00",
  Cuba: "#002a8f",
  Finland: "#003580",
  France: "#ed2939",
  Germany: "#000",
  Iceland: "#003897",
  India: "#f93",
  Japan: "#bc002d",
  NorthKorea: "#024fa2",
  SouthKorea: "#000",
  NewZealand: "#00247d",
  Norway: "#ef2b2d",
  Poland: "#dc143c",
  Russia: "#d52b1e",
  Turkey: "#e30a17",
  UnitedKingdom: "#00247d",
  UnitedStates: "#b22234",
};

function getFlag(cycle_itemname) {
  if (!cycle_itemname) {
    return "";
  }
  return (
    recycle_category_name.find(function (item) {
      return item.name === cycle_itemname;
    }) || {}
  ).emoji;
}

const feedback_calculate = [
  //金屬廢料混合物((熱處理/物理)
  {
    findnum: 4 ,
    back_step : 1
  },
  // 回收NMP/NMP (黑) 
  {
    findnum: 6 ,
    back_step : 1
  },
   //含鋁混合五金廢料(卷料/邊片料)
  {
    findnum: 8 ,
    back_step : 1
  },
  //含銅混合五金廢料(卷料/邊片料)
  {
    findnum: 10 ,
    back_step : 1
  },
  //廢塑膠(鋁塑膜與PP)
  {
    findnum: 14 ,
    back_step : 1
  },
]

const Analyticsyear = () => {
  const chartRef = useRef(null); // 创建 ref 来引用 DOM 元素
  const chartRefs = useRef([]);
  const chartInstances = useRef([]); // 保存 ECharts 實例
  const [chartDataList, setchartDataList] = useState([]);
  const [newfield, setNewField] = useState(true); // true: 用新規範 , false: 延續舊設計規範
  const currentYear = new Date().getFullYear();
  const record_yearlen = parseInt(currentYear) - 2025;

  // const years = [];

  // // 生成從至今到2年前的年分
  // for (let i = 0; i >= -2; i--) {
  //   years.push(currentYear + i);
  // }

  const years = Array.from(
    { length: record_yearlen + 1 },
    (_, i) => currentYear - i
  );

  const [selectedYear, setSelectedYear] = useState("");
  const [chartyearamont, setchartyearamont] = useState([]);


  const convert_sortarray = (search_idx , chartData_all) => {

    const run_actionarray = newfield ? recycle_category_name : recycle_category_name_old;
    
    console.log("實際取得轉換欄位:"+ JSON.stringify(run_actionarray,null,2));

    const result = run_actionarray.map((name, i) => [
      name,
      Number(chartData_all[i]) || 0,
    ]);

    return Number(search_idx) === 0
      ? result.sort((a, b) => b[1] - a[1])
      : result;

    // return Number(search_idx) === 0 ? 
    //            run_actionarray
    //                 .map((name, i) => [name, chartData_all[i]]) // 合併為 [name, value]
    //                 .sort((a, b) => b[1] - a[1]) // 按值排序                    
    //             : run_actionarray.map((name, i) => [name, chartData_all[i]]) // 保持原順序            
  }

  // 從 localStorage 中讀取選擇的年分
  useEffect(() => {
    let myChart;
    let data = [];

    // for (let i = 0; i < 12; ++i) {
    //   data.push(Math.round(Math.random() * 200));
    // }
    // for (let i = 0; i < 10; ++i) {
    //   data.push(Math.round(Math.random() * 200));
    // }

    // for (let i = 0; i < data.length; ++i) {
    //   if (
    //     yearsdata.length === 0 ||
    //     yearsdata[yearsdata.length - 1] !== data[i]
    //   ) {
    //     yearsdata.push(data[i]);
    //   }
    // }

    // let startYear = yearsdata[startIndex];

    const savedYear = localStorage.getItem("selectedYear");
    if (savedYear === null) {
      //預設為當前年份為選單
      setSelectedYear(currentYear);
    }

    const ReflashRecycleChart = async () => {
      try {
        const response = await axios.get(
           `${config.apiBaseUrl}/recycle/getyearamont`,
          //   "http://localhost:3009/recycle/getyearamont",
          {
            params: {
              year: currentYear,
            },
          }
        );

        console.log(response.data[0].totalamont);
        console.log(response.data[1].total_negative_amont);
        console.log(response.data[2].total_positive_amont);

        //確保有3組數據回傳接收
        if (response.status === 200 && response.data.length === 3) {
          //清空原先陣列
          setchartDataList([]);

          const formattedList = response.data.map((item, index) => {
            const key = Object.keys(item || {})[0];
            const rawData = item?.[key] ?? "";

            const dataArray =
              typeof rawData === "string"
                ? rawData.split(",").map((val ,index) => {
                const recycle_val  =  parseFloat(val) || 0;
                return recycle_val;
              }): [];

            const resultArray = [...dataArray];

            // 不修改原始 dataArray , 後續合併區間依照原始 index 判斷，不受陣列長度改變影響。
            // 依照原始 index 合併後重新組合
            // const resultArray2 = [];
            // const mergeMap = new Map();
            
            // // 1. 依照原始 dataArray index 建立合併區間
            // feedback_calculate.forEach(({ findnum, back_step }) => {
            //   const startIndex = findnum - back_step;

            //   mergeMap.set(startIndex, findnum);
            // });

            // // 2. 將合併區間依起始 index 由大到小排序
            // const mergeRanges = [...mergeMap.entries()]
            //   .map(([startIndex, endIndex]) => ({
            //     startIndex,
            //     endIndex,
            //   }))
            //   .sort((a, b) => b.startIndex - a.startIndex);

            // // 3. 從最後一個合併區間往前計算
            // const mergedValues = new Map();

            // mergeRanges.forEach(({ startIndex, endIndex }) => {
            //   let total = 0;

            //   for (let j = startIndex; j <= endIndex; j++) {
            //     total += Number(dataArray[j] ?? 0);
            //   }

            //   // 合併結果存放於原始起始 index
            //   mergedValues.set(startIndex, total);
            // });

            // let i = 0;

            // while (i < dataArray.length) {
            //   if (mergedValues.has(i)) {
            //     const endIndex = mergeMap.get(i);
            //     resultArray2.push(mergedValues.get(i));

            //     // 跳過已合併的原始 index
            //     i = endIndex + 1;

            //   } else {
            //     resultArray2.push(dataArray[i]);
            //     i++;
            //   }
            // }
            
            //針對新標籤名做內部同屬性class回收項目做合併
            //金屬廢料混合物(熱處理) , 金屬廢料混合物(物理)  (序號: 3,4)
            //底料NMP ,E004NMP(回收)  (序號: 5,6)
            // 含鋁,銅混和五金廢料 (卷料, 邊/片料) (序號: 7,8,9,10 )
            // 廢塑膠 < 紙箱含塑膠混和物, 鋁塑膜 , PP膜> (序號: 12,13,14 )
            // 讓 feedback_calculate 從最大的 index 開始處理，前面的 splice 就不會影響後面的索引
            // PS: 若持續使用既有順向訪問, 後續產生問題是 每做一次 splice，後面的 index 全部往前移
            [...feedback_calculate]
            .sort((a, b) => b.findnum - a.findnum)
            .forEach(({ findnum, back_step }, index) => {
              const startIndex = findnum - back_step;
              const endIndex = findnum;

              // 計算合併區間總和
              let total_caculator = 0;

              for (let i = startIndex; i <= endIndex; i++) {
                total_caculator += resultArray[i];
              }

              // 將合併結果存回起始位置
              resultArray[startIndex] = total_caculator;
               
              // 移除起始位置之後、直到 findnum 的元素(起始位置,沿生數量)
              resultArray.splice(startIndex + 1, endIndex - startIndex);
              
            });

            return {
              id: `chart${index + 1}`,
              label: keyNameMap?.[key] || key || `Chart ${index + 1}`,
              data: newfield ?resultArray:dataArray,
            };
          });

          // 更新狀態
          console.log("更新狀態 formattedList:", formattedList);
          setchartDataList(formattedList);

          // setchartyearamont(formattedList[0].data);
        }

        chartRefs.length = 0; // 清空陣列

        // 初始化图表
        // myChart = echarts.init(chartRef.current);

        myChart.on("mouseover", function (params) {
          if (params.componentType === "series") {
            myChart.dispatchAction({
              type: "showTip",
              seriesIndex: params.seriesIndex,
              dataIndex: params.dataIndex,
            });
          }
        });

        myChart.on("mouseout", function () {
          myChart.dispatchAction({ type: "hideTip" });
        });

        option = {
          title: {
            text: "年全項目累積總量",
            left: "center", // 可選：讓標題置中
            top: 10, // 可選：調整上下位置
            bottom: 10, // 可選：調整上下位置
            textStyle: {
              fontSize: 5,
              fontWeight: "bold",
            },
          },
          grid: {
            top: 100,
            bottom: 100,
            left: 150,
            right: 250,
          },
          tooltip: {
            order: "valueDesc",
            // trigger: "axis",
            trigger: "item",
            // axisPointer: {
            //   type: "cross",
            //   textStyle: {
            //     align: "left",
            //   },
            // },
            // formatter: function (param) {
            //   let htmlStr = "";
            //   for (let i = 0; i < param.length; i++) {
            //     const xName = param[i].name;
            //     const seriesName = param[i].seriesName;
            //     const value = param[i].value;
            //     const color = param[i].color;

            //     if (i === 0) {
            //       htmlStr += xName + "<br/>";
            //     }

            //     htmlStr += "<div>";

            //     htmlStr += '<div style="board:1px solid #FFEB3B"></div>';
            //     htmlStr += "年總累積量:" + value / 1000 + " 公噸(tonne)/單位";
            //     htmlStr += '<div style="board:1px solid #FFEB3B"></div>';
            //     htmlStr += "</div>";
            //   }
            //   return htmlStr;
            // },
            formatter: function (params, index) {
              // return (
              //   "<div>" +
              //   params[0].name +
              //   params[0].marker +
              //   params[0].seriesname +
              //   ":" +
              //   '<span style="color: #00B83F;">' +
              //   params[0].value +
              //   "</span>公斤/單位" +
              //   "</div>"
              // );
              let name = params.name;
              let value;

              if (Array.isArray(params.value)) {
                // 根據 encode 決定 value 在哪
                value = index === 0 ? params.value[1] : params.value[0];
              } else {
                value = params.value;
              }

              return `${name}：${Number(value).toLocaleString()}`;

              //return `${params.series}`;
              // return (
              //   // "X: " +
              //   // params.data[0].toFixed(3) +
              //   // "<br />Y: " +
              //   "<br /> 年總累積量: " + params.series.data[dataIndex].toFixed(3)
              // );
            },
          },
          xAxis: {
            max: "dataMax",
            axisLabel: {
              formatter: function (n) {
                return Math.round(n) + "";
              },
            },
          },
          // dataset: {
          //   source: data.slice(1).filter(function (d) {
          //     return d[4] === startYear;
          //   }),
          // },
          yAxis: {
            type: "category",
            // data: ["A", "B", "C", "D", "E"],
            // data: [
            //   "廢塑膠混合物",
            //   "廢木材棧板",
            //   "非有害油泥",
            //   "金屬廢料混合物(熱處理)",
            //   "金屬廢料混合物(物理)",
            //   "底料NMP",
            //   "E004NMP(回收)",
            //   "含鋁混和五金廢料(卷料)",
            //   "含鋁混和五金廢料(邊/片料)",
            //   "含銅混和五金廢料(卷料)",
            //   "含銅混和五金廢料(邊/片料)",
            //   "廢電子零組件",
            //   "廢塑膠(紙箱含塑膠混和物)",
            //   "廢塑膠(鋁塑膜)",
            //   "廢塑膠(PP膜)",
            //   "廢銅",
            //   "廢鋁",
            //   "廢乾電池",
            // ],
            data: newfield? recycle_category_name : recycle_category_name_old,
            inverse: true,
            animationDuration: 200,
            animationDurationUpdate: 300,
            max: 11, // only the largest 12 will be displayed
            // inverse: true,
            // max: 10,
            // axisLabel: {
            //   show: true,
            //   fontSize: 14,
            //   formatter: function (value) {
            //     return value + "{flag|" + getFlag(value) + "}";
            //   },
            //   rich: {
            //     flag: {
            //       fontSize: 25,
            //       padding: 5,
            //     },
            //   },
            // },
            // animationDuration: 300,
            // animationDurationUpdate: 300,
          },
          series: [
            {
              realtimeSort: true,
              // seriesLayoutBy: "column",
              name: "year",
              type: "bar",
              data: chartyearamont,
              itemStyle: {
                color: function (param) {
                  return countryColors[param.value[0]] || "#5470c6";
                },
              },
              encode: {
                x: dimension,
                y: 5,
              },
              label: {
                show: true,
                silent: true,
                precision: 2,
                position: "right",
                valueAnimation: true,
                fontFamily: "monospace",
              },
            },
          ],
          legend: {
            show: true,
          },
          // Disable init animation.
          animationDuration: 0,
          animationDurationUpdate: updateFrequency,
          animationEasing: "linear",
          animationEasingUpdate: "linear",
          // graphic: echarts.util.map(chartyearamont, function (item, dataIndex) {
          //   return {
          //     type: "text",
          //     left: "center",
          //     bottom: 20,
          //     style: {
          //       text: selectedYear,
          //       font: "bolder 80px monospace",
          //       fill: "rgba(10, 10, 10, 0.05)",
          //     },
          //     z: 1,
          //     onmousemove: echarts.util.curry(showTooltip, dataIndex),
          //     onmouseout: echarts.util.curry(hideTooltip, dataIndex),
          //   };
          // }),
        };

        // myChart.setOption(option);
        // for (let i = startIndex; i < years.length - 1; ++i) {
        //   (function (i) {
        //     setTimeout(function () {
        //       updateYear(years[i + 1]);
        //     }, (i - startIndex) * updateFrequency);
        //   })(i);
        // }
        // function updateYear(year) {
        //   let source = data.slice(1).filter(function (d) {
        //     return d[4] === year;
        //   });
        //   option.series[0].data = source;
        //   option.graphic.elements[0].style.text = year;
        //   myChart.setOption(option);
        // }

        option && myChart.setOption(option);

        window.addEventListener("resize", myChart.resize);
      } catch (error) {
        console.error("取得資料錯誤", error);
      }
    };

    ReflashRecycleChart();

    // 清理工作：组件卸载时销毁图表实例
    return () => {
      if (myChart) {
        myChart.dispose();
      }
    };
  }, []);

  function getAlignedChartData(
    chartData,
    index,
    categoryList,
    getLabel = (item) => item.name,
    getValue = (item) => item.value
  ) {
    if (index === 0) {
      // 排序：由大到小
      return [...chartData].sort((a, b) => getValue(b) - getValue(a));
    }

    // 用標準化的 label 當作 key（去除空白）
    const map = new Map(chartData.map((item) => [getLabel(item).trim(), item]));

    return categoryList.map((label) => {
      return map.get(label.trim()) || { name: label, value: 0 };
    });
  }

  const renderRecycle_Chart = async ({
    chartRef,
    chartData,
    selectedYear,
    dimension,
    charlabel,
    index,
  }) => {
    let myChart;
    try {
      if (!chartRef.current) return;

      if (chartRef.current) {
        const instance = echarts.getInstanceByDom(chartRef.current);
        if (instance) {
          instance.dispose();
        }
      }

      // 初始化圖表
      myChart = echarts.init(chartRef.current);

      // myChart.on("mouseover", function (params) {
      //   if (params.componentType === "series") {
      //     myChart.dispatchAction({
      //       type: "showTip",
      //       seriesIndex: params.seriesIndex,
      //       dataIndex: params.dataIndex,
      //     });
      //   }
      // });

      // myChart.on("mouseout", function () {
      //   myChart.dispatchAction({ type: "hideTip" });
      // });

      // const alignedChartData = getAlignedChartData(
      //   chartData,
      //   index,
      //   recycle_category_name,
      //   dimension
      // );

      const option = {
        title: {
          text: "年全項目累積總量",
          left: "center",
          top: 10,
          textStyle: {
            fontSize: 5,
            fontWeight: "bold",
          },
        },
        grid: {
          top: 100,
          bottom: 100,
          left: 150,
          right: 250,
        },
        tooltip: {
          trigger: "item",
          axisPointer: {
            type: "cross",
          },
          formatter: function (params) {
            const item = params[0];
            const value = Array.isArray(item.value)
              ? item.value[1]
              : item.value;
            return `${item.name}：${Number(value).toLocaleString()}`;
          },
        },
        xAxis:
          index === 0
            ? {
                type: "category",
                data: newfield ? recycle_category_name : recycle_category_name_old , // 類別名稱陣列
                axisLabel: {
                  interval: 0,
                  rotate: 30, // 將文字旋轉 90 度（垂直）
                  fontSize: 12, // 可選：調整字體大小
                  margin: 8, // 可選：與軸線間距
                },
              }
            : {
                type: "value",
                max: "dataMax",
                axisLabel: {
                  formatter: function (n) {
                    return Math.round(n) + "";
                  },
                },
              },
        yAxis:
          index === 0
            ? {
                type: "value",
              }
            : {
                type: "category",
                data: newfield ? recycle_category_name : recycle_category_name_old , // 類別名稱陣列
                inverse: true,
                animationDuration: 200,
                animationDurationUpdate: 300,
                max: 17,
              },
        legend: {
          show: true,
          data: [charlabel || "年全項目累積總量"],
          top: 30,
        },
        series: [
          {
            realtimeSort: index === 0, // 只有 index === 0 才開啟自動排序動畫
            // seriesLayoutBy: "column",
            name: [charlabel || "年全項目累積總量"],
            type: "bar",
            // data: alignedChartData,
            data: 
               convert_sortarray(index,chartData),
              // index === 0
              //   ? recycle_category_name
              //       .map((name, i) => [name, chartData[i]]) // 合併為 [name, value]
              //       .sort((a, b) => b[1] - a[1]) // 按值排序                    
              //   : recycle_category_name.map((name, i) => [name, chartData[i]]), // 保持原順序
            itemStyle: {
              color: function (param) {
                return (
                  countryColors[param.value[0]] ||
                  (index === 0
                    ? "#5470c6"
                    : index === 1
                    ? "#ff0000"
                    : "#00ff00")
                );
              },
            },

            encode: {
              x: index === 0 ? 0 : 1, // index 0: name 在 x，index > 0: value 在 x
              y: index === 0 ? 1 : 0, // index 0: value 在 y，index > 0: name 在 y
            },

            label: {
              show: true,
              precision: 1,
              position: index === 0 ? "top" : "right",
              valueAnimation: true,
              fontFamily: "monospace",
              silent: false,
              // position: "top", // ✅ 數值顯示在柱子頂部
            },
          },
        ],

        animationDuration: 0,
        animationDurationUpdate: updateFrequency,
        animationEasing: "linear",
        animationEasingUpdate: "linear",
        // graphic: echarts.util.map(chartData, function (item, dataIndex) {
        //   return {
        //     type: "text",
        //     right: 160,
        //     top: 10,
        //     style: {
        //       text: selectedYear,
        //       font: "bolder 50px monospace",
        //       fill: "rgba(100, 100, 100, 0.25)",
        //     },
        //     z: 100,
        //     onmousemove: echarts.util.curry(showTooltip, dataIndex),
        //     onmouseout: echarts.util.curry(hideTooltip, dataIndex),
        //   };
        // }),
      };

      option && myChart.setOption(option);

      // function showTooltip(dataIndex) {
      //   myChart.dispatchAction({
      //     type: "showTip",
      //     seriesIndex: 0,
      //     dataIndex: dataIndex,
      //   });
      // }

      // function hideTooltip(dataIndex) {
      //   myChart.dispatchAction({
      //     type: "hideTip",
      //   });
      // }
    } catch (error) {
      console.error("圖表渲染錯誤", error);
    }
  };

  //當有接收到新的 chartDataList 時，重新渲染所有圖表
  useEffect(() => {
    if (!chartDataList || chartDataList.length === 0) return;
    chartDataList.forEach((item, index) => {
      // console.log("渲染圖表:", item.label, "索引:", index);
      renderRecycle_Chart({
        chartRef: { current: chartRefs.current[index] },
        chartData: item.data,
        selectedYear,
        charlabel: item.label,
        index,
      });
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [chartDataList, selectedYear]);

  const afterReflashRecycleChart = async (e) => {
    let myChart;

    try {
      if (echarts.getInstanceByDom(chartRef.current)) {
        echarts.getInstanceByDom(chartRef.current).dispose();
      }

      // 初始化图表
      myChart = echarts.init(chartRef.current);

      option = {
        title: {
          text: "年全項目累積總量",
          left: "center", // 可選：讓標題置中
          top: 10, // 可選：調整上下位置
          textStyle: {
            fontSize: 5,
            fontWeight: "bold",
          },
        },
        grid: {
          top: 100,
          bottom: 100,
          left: 150,
          right: 250,
        },
        tooltip: {
          order: "valueDesc",
          // trigger: "axis",
          trigger: "item",
          axisPointer: {
            type: "cross",
            textStyle: {
              align: "left",
            },
          },
          // },
          // formatter: function (param) {
          //   let htmlStr = "";
          //   for (let i = 0; i < param.length; i++) {
          //     const xName = param[i].name;
          //     const seriesName = param[i].seriesName;
          //     const value = param[i].value;
          //     const color = param[i].color;

          //     if (i === 0) {
          //       htmlStr += xName + "<br/>";
          //     }

          //     htmlStr += "<div>";

          //     htmlStr += '<div style="board:1px solid #FFEB3B"></div>';
          //     htmlStr += "年總累積量:" + value / 1000 + " 公噸(tonne)/單位";
          //     htmlStr += '<div style="board:1px solid #FFEB3B"></div>';
          //     htmlStr += "</div>";
          //   }
          //   return htmlStr;
          // },

          formatter: function (params, dataIndex) {
            // return (
            //   "<div>" +
            //   params[0].name +
            //   params[0].marker +
            //   params[0].seriesname +
            //   ":" +
            //   '<span style="color: #00B83F;">' +
            //   params[0].value +
            //   "</span>公斤/單位" +
            //   "</div>"
            // );
            return `${params.name}: ${params.value}`;
            // return `${params.series}`;
            // return (
            //   // "X: " +
            //   // params.data[0].toFixed(3) +
            //   // "<br />Y: " +
            //   "<br /> 年總累積量: " + params.series.data[dataIndex].toFixed(3)
            // );
          },
        },
        xAxis: {
          max: "dataMax",
          axisLabel: {
            formatter: function (n) {
              return Math.round(n) + "";
            },
          },
        },
        // dataset: {
        //   source: data.slice(1).filter(function (d) {
        //     return d[4] === startYear;
        //   }),
        // },
        yAxis: {
          type: "category",
          // data: ["A", "B", "C", "D", "E"],
          data: newfield? recycle_category_name : recycle_category_name_old,
          inverse: true,
          animationDuration: 200,
          animationDurationUpdate: 300,
          max: 17, // only the largest 12 will be displayed
          // inverse: true,
          // max: 10,
          // axisLabel: {
          //   show: true,
          //   fontSize: 14,
          //   formatter: function (value) {
          //     return value + "{flag|" + getFlag(value) + "}";
          //   },
          //   rich: {
          //     flag: {
          //       fontSize: 25,
          //       padding: 5,
          //     },
          //   },
          // },
          // animationDuration: 300,
          // animationDurationUpdate: 300,
        },
        series: [
          {
            realtimeSort: true,
            //  seriesLayoutBy: "column",
            name: "年全項目累積總量",
            type: "bar",
            data: chartyearamont,
            itemStyle: {
              color: function (param) {
                return countryColors[param.value[0]] || "#5470c6";
              },
            },
            encode: {
              x: dimension,
              y: 5,
            },
            label: {
              show: true,
              precision: 2,
              position: "right",
              valueAnimation: true,
              fontFamily: "monospace",
            },
          },
        ],
        legend: {
          show: true,
        },
        // Disable init animation.
        animationDuration: 0,
        animationDurationUpdate: updateFrequency,
        animationEasing: "linear",
        animationEasingUpdate: "linear",
        // graphic: echarts.util.map(chartyearamont, function (item, dataIndex) {
        //   return {
        //     type: "text",
        //     right: 160,
        //     bottom: 1,
        //     style: {
        //       text: selectedYear,
        //       font: "bolder 80px monospace",
        //       fill: "rgba(100, 100, 100, 0.25)",
        //     },
        //     z: 100,
        //     onmousemove: echarts.util.curry(showTooltip, dataIndex),
        //     onmouseout: echarts.util.curry(hideTooltip, dataIndex),
        //   };
        // }),
      };

      // myChart.setOption(option);
      // for (let i = startIndex; i < years.length - 1; ++i) {
      //   (function (i) {
      //     setTimeout(function () {
      //       updateYear(years[i + 1]);
      //     }, (i - startIndex) * updateFrequency);
      //   })(i);
      // }
      // function updateYear(year) {
      //   let source = data.slice(1).filter(function (d) {
      //     return d[4] === year;
      //   });
      //   option.series[0].data = source;
      //   option.graphic.elements[0].style.text = year;
      //   myChart.setOption(option);
      // }

      option && myChart.setOption(option);
    } catch (error) {
      console.error("取得資料錯誤", error);
    }

    function showTooltip(dataIndex) {
      myChart.dispatchAction({
        type: "showTip",
        seriesIndex: 0,
        dataIndex: dataIndex,
      });
    }

    function hideTooltip(dataIndex) {
      myChart.dispatchAction({
        type: "hideTip",
      });
    }
  };

  const handleChange = async (e) => {
    const { name, value } = e.target;

    if (name === "yearselect") {
      setSelectedYear(value);
      // console.log("yearselect value = " + value);
      try {
        //2024-09-25 API 這邊要重寫對應之後台
        const response = await axios.get(
          `${config.apiBaseUrl}/recycle/getyearamont`,
         //  "http://localhost:3009/recycle/getyearamont",
          {
            params: {
              year: value,
            },
          }
        );

        // console.log(response.data.length);
        // console.log(response.data[0].totalamont);
        // console.log(response.data[1].total_negative_amont);
        // console.log(response.data[2].total_positive_amont);

        if (response.status === 200 && response.data.length === 3) {
          // 将字符串分割为数组并转换为浮点数
          // const dataArray = response.data[0].totalamont.split(",").map(Number);
          // setchartyearamont(dataArray);

          //清空原先陣列
          setchartDataList([]);

          const formattedList = response.data.map((item, index) => {
            const key = Object.keys(item || {})[0];
            const rawData = item?.[key] ?? "";

            const dataArray =
              typeof rawData === "string"
                ? rawData.split(",").map((val) => parseFloat(val) || 0)
                : [];


            const resultArray = [...dataArray];
            
            //針對新標籤名做內部同屬性class回收項目做合併
            //金屬廢料混合物(熱處理) , 金屬廢料混合物(物理)  (序號: 3,4)
            //底料NMP ,E004NMP(回收)  (序號: 5,6)
            // 含鋁,銅混和五金廢料 (卷料, 邊/片料) (序號: 7,8,9,10 )
            // 廢塑膠 < 紙箱含塑膠混和物, 鋁塑膜 , PP膜> (序號: 12,13,14 )
            // 讓 feedback_calculate 從最大的 index 開始處理，前面的 splice 就不會影響後面的索引
            // PS: 若持續使用既有順向訪問, 後續產生問題是 每做一次 splice，後面的 index 全部往前移
            [...feedback_calculate]
            .sort((a, b) => b.findnum - a.findnum)
            .forEach(({ findnum, back_step }, index) => {
              const startIndex = findnum - back_step;
              const endIndex = findnum;

              // 計算合併區間總和
              let total_caculator = 0;

              for (let i = startIndex; i <= endIndex; i++) {
                total_caculator += resultArray[i];
              }

              // 將合併結果存回起始位置
              resultArray[startIndex] = total_caculator;
               
              // 移除起始位置之後、直到 findnum 的元素(起始位置,沿生數量)
              resultArray.splice(startIndex + 1, endIndex - startIndex);              
            });

            return {
                id: `chart${index + 1}`,
                label: keyNameMap?.[key] || key || `Chart ${index + 1}`,            
                data: newfield ?resultArray:dataArray,              
              };
          });
          
          // 更新狀態
          console.log("更新狀態 formattedList:", formattedList);
          setchartDataList(formattedList);

          // afterReflashRecycleChart();
        }
      } catch (error) {
        console.error("Error fetching selectyear:", error);
      }
    }
  };

  return (
    <Form>
      <div>
        <h1
          style={{
            fontSize: "3.6rem",
            marginBottom: "10px",
            textAlign: "center",
            color: "#130f0fff",
            marginTop: "10px",
            padding: "2px",
          }}
        >
          資源回收年數據分析
        </h1>
        <label>
          選擇年份：
          <select
            name="yearselect"
            value={selectedYear}
            onChange={handleChange}
          >
            {/* <option value="">請選擇</option> */}
            {years.map((year) => (
              <option key={year} value={year}>
                {year}
              </option>
            ))}
          </select>
        </label>
        {/* {selectedYear && <p>您選擇的年份是: {selectedYear}</p>} */}
      </div>
      {/* <div ref={chartRef} style={{ width: "100%", height: "600px" }}></div> */}
      {chartDataList.map((item, index) => (
        <React.Fragment key={item.label}>
          <span
            style={{
              fontWeight: "bold",
              display: "block",
              fontSize: "30px",
              color: "#130f0fff",
              textAlign: "center",
              padding: "2px",
              marginBottom: "2px",
            }}
          >
            {item.label} {"▽"}
          </span>
          <div
            ref={(el) => (chartRefs.current[index] = el)}
            style={{ width: "100%", height: "600px", marginTop: "12px" }}
          />
        </React.Fragment>
      ))}
    </Form>
  );
};

export default Analyticsyear;

const CustomTooltip = ({ active, payload, label }) => {
  if (active && payload && payload.length) {
    return (
      <div className="p-4 bg-slate-900 flex flex-col gap-4 rounded-md">
        <p className="text-medium text-lg">{label}</p>
        <p className="text-sm text-blue-400">
          Revenue:
          <span className="ml-2">${payload[0].value}</span>
        </p>
        <p className="text-sm text-indigo-400">
          Profit:
          <span className="ml-2">${payload[1].value}</span>
        </p>
      </div>
    );
  }
};
