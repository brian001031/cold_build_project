/* eslint-disable react-hooks/rules-of-hooks */
import React, { useState, useEffect, useRef } from "react";
import Form from "react-bootstrap/Form";
import config from "../../../config";
import DynamicTable from "../../../components/DynamicTable";
import axios from "axios";
import { Button } from "react-bootstrap";
//成功提示套件
import { toast } from "react-toastify";
import * as echarts from "echarts/core";
import {
  DatasetComponent,
  GraphicComponent,
  GridComponent,
  TitleComponent,
  ToolboxComponent,
  TooltipComponent,
  LegendComponent,
} from "echarts/components";
import { SVGRenderer, CanvasRenderer } from "echarts/renderers";
import { LineChart, PieChart, BarChart } from "echarts/charts";
import { UniversalTransition, LabelLayout } from "echarts/features";
import "./index.scss";

echarts.use([
  TitleComponent,
  ToolboxComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
  LineChart,
  CanvasRenderer,
  UniversalTransition,
  DatasetComponent,
  GraphicComponent,
  PieChart,
  LabelLayout,
  BarChart,
]);

echarts.use([SVGRenderer, CanvasRenderer]);

let tabledataconvert = [];
let startIndex = 10;

let currentYear = new Date().getFullYear();
//原先字符字串佔2位元會影響初始化顯示,改用原先不padStart指定狀態
// let currentMonth = (new Date().getMonth() + 1).toString().padStart(2, "0");
let currentMonth = new Date().getMonth() + 1;

const posList = [
  "left",
  "right",
  "top",
  "bottom",
  "inside",
  "insideTop",
  "insideLeft",
  "insideRight",
  "insideBottom",
  "insideTopLeft",
  "insideTopRight",
  "insideBottomLeft",
  "insideBottomRight",
];

const recycleItem = [
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

const dynamicday = () => {
  const chartRef = useRef(null); // 创建 ref 来引用 DOM 元素 (指定月份全日期)
  const chartRef2 = useRef(null); // 创建 ref 来引用 DOM 元素 (指定月份)
  const [selectedYear, setSelectedYear] = useState("");
  const [selectedMonth, setSelectedMonth] = useState("");
  const [dayout_amont, setdayout_amont] = useState([]);
  const [specifymonth_amont, setSpecifymonth_amont] = useState([]);
  const [encode_itemName, setencode_itemName] = useState("");
  const [encode_value, setencode_value] = useState("");
  const [showdisplayTable, setdisplayTable] = useState(false);
  const [chartsource, setChartSource] = useState([]);
  const [checklabelOption, setChecklabelOption] = useState(false);

  // 定義 configParameters 狀態 (資源回收項目月總數據使用only)
  const [configParameters, setConfigParameters] = useState({
    rotate: { min: -90, max: 90 },
    align: { options: { left: "left", center: "center", right: "right" } },
    verticalAlign: {
      options: { top: "top", middle: "middle", bottom: "bottom" },
    },
    position: {
      options: posList.reduce((map, pos) => {
        map[pos] = pos;
        return map;
      }, {}),
    },
    distance: { min: 0, max: 100 },
  });

  // 定義 config 狀態 (資源回收項目月總數據使用only)
  const [amountconfig, set_AmountConfig] = useState({
    rotate: 90,
    align: "left",
    verticalAlign: "left",
    position: "insideBottom",
    distance: 7,
  });

  const labelOption_Init = {
    show: true,
    position: amountconfig.position,
    distance: amountconfig.distance,
    align: amountconfig.align,
    verticalAlign: amountconfig.verticalAlign,
    rotate: amountconfig.rotate,
    formatter: "{c}  {name|{a}}",
    fontSize: 16,
    rich: {
      name: {},
    },
  };

  const viewchartevent = () => {
    //setdisplayTable((prev) => !prev);
    setdisplayTable(true);
  };

  // eslint-disable-next-line no-unused-vars
  let myChart2;
  // eslint-disable-next-line no-unused-vars
  let option2;
  const years = [];
  const months = [];

  // const sampleData = [
  //   {
  //     name: "Alice",
  //     age: 25,
  //     city: "New York",
  //     language: "English",
  //     TT: "Alice",
  //     SS: 25,
  //     CD: "New York",
  //     OP: "English",
  //     pp: "Alice",
  //     ff: 25,
  //     ll: "New York",
  //     ppp: "English",
  //     err: "Alice",
  //     popo: 25,
  //     wwwOP: "English",
  //   },
  //   { name: "Bob", age: 30, city: "Tokyo", language: "Japan" },
  //   { name: "Charlie", age: 35, city: "Taipei", language: "Chinses(Big5)" },
  //   { name: "Charlie", age: 35, city: "Taipei", language: "Chinses(Big5)" },
  //   { name: "Charlie", age: 35, city: "Taipei", language: "Chinses(Big5)" },
  //   { name: "Charlie", age: 35, city: "Taipei", language: "Chinses(Big5)" },
  //   { name: "Charlie", age: 35, city: "Taipei", language: "Chinses(Big5)" },
  // ];

  // 生成從至今到2年前的年分
  for (let i = 0; i >= -2; i--) {
    years.push(currentYear + i);
  }

  for (let m = 1; m <= 12; m++) {
    months.push(m);
  }

  useEffect(() => {
    // if (chartyearamont) {
    //   afterReflashRecycleChart();
    // } else {
    //   setchartyearamont([]);
    // }
    const savedYear = localStorage.getItem("selectedYear");
    const savedMonth = localStorage.getItem("selectedMonth");

    // console.log("當前年份:" + currentYear);
    // console.log("當前月份:" + currentMonth);

    if (savedYear !== currentYear || savedMonth !== currentMonth) {
      // console.log("有切換當前年月份調整-----");
      //預設為當前年份為選單
      setSelectedYear(currentYear);
      setSelectedMonth(currentMonth);

      // try {
      //   const response = axios.get(
      //     //`${config.apiBaseUrl}/recycle/getall_dateinfo`,
      //     "http://localhost:3009/recycle/getall_dateinfo",
      //     {
      //       params: {
      //         selectyear: selectedYear, // 這邊搜尋年月回饋指定全月份每天的提交回收全部紀錄
      //         selectmonth: selectedMonth,
      //       },
      //     }
      //   );

      //   const amont_totaldayout = response.data;

      //   if (response.status === 210) {
      //     //toast.success(`搜尋amont_totaldayout成功.`);
      //     // 将字符串分割为数组并转换为浮点数
      //     //const dataArray = response.data.split(",").map(Number);
      //     setdayout_amont(amont_totaldayout);

      //     //Provide_Pie_Line_RecycleChart();
      //   } else if (response.status === 404) {
      //     toast.success(`搜尋amont_totaldayout 失敗`);
      //   }
      // } catch (error) {
      //   console.error("Error fetching data:", error);
      // }
    }
  }, []);

  //觸發計算每個項目指定月全日期每個總量
  useEffect(() => {
    //這邊代表有搜尋到回收數據量並重新整理回傳到(chart,table)顯示畫面
    if (dayout_amont) {
      const encodestr = dayout_amont.toLocaleString().split(",");
      setencode_itemName(encodestr[0]);
      setencode_value(encodestr[1]);

      tabledataconvert = dayout_amont.map((item) => {
        const values = item.split(",");
        const obj = {};
        values.forEach((value, index) => {
          obj[`日期:${index + 0}`] = value;
        });
        return obj;
      });

      const transformedData = dayout_amont.map((item) => {
        // 拆分字符串為數組
        const values = item.split(",");
        // 將數組返回為數據行
        return values.map((value, index) => {
          // 假設日期在索引1及以上，其他為數值
          if (index === 0) {
            return value; // 物品名稱
          } else if (isNaN(value)) {
            return value; // 日期保持為字串
          } else {
            return Number(value); // 將數字轉為數字型別
          }
        });
      });

      setChartSource(transformedData);

      //console.log("轉換tabledataconvert 為 = " + tabledataconvert);
      //console.log("轉換transformedData 為 = " + chartsource);
    } else {
      //重新初始化內部存值
      setdayout_amont([]);
      setencode_itemName("");
      setencode_value("");
    }
  }, [dayout_amont]);

  //觸發計算每個項目月總量
  useEffect(() => {
    if (specifymonth_amont) {
      // console.log(
      //   "specifymonth_amont 觸發JSON為 = " +
      //     JSON.stringify(specifymonth_amont) +
      //     "specifymonth_amont 觸發陣列為 = " +
      //     specifymonth_amont
      // );

      Provide_BarchartMonth_RecycleChart();
    } else {
      setSpecifymonth_amont([]);
    }
  }, [specifymonth_amont]);

  useEffect(() => {
    if (encode_itemName && encode_value && chartsource) {
      console.log(encode_itemName);
      console.log(encode_value);
      console.log(chartsource);

      Provide_Pie_Line_RecycleChart();
    }
  }, [encode_itemName, encode_value, chartsource]);

  const handleChange = async (e) => {
    const { name, value } = e.target;

    if (name === "yearselect") {
      setSelectedYear(Number(value));
    } else if (name === "monthselect") {
      setSelectedMonth(Number(value));
    } else if (name === "month_amount") {
      Provide_BarchartMonth_Adjust();
    }
  };

  const Provide_BarchartMonth_Adjust = async (e) => {
    // e.preventDefault(); // 防止默認的表單提交行為

    console.log("Provide_BarchartMonth_Adjust 有進來動作");
    // 定義 labelOption 狀態 (資源回收項目月總數據使用only)
    const labelOption = {
      rotate: amountconfig.rotate,
      align: amountconfig.align,
      verticalAlign: amountconfig.verticalAlign,
      position: amountconfig.position,
      distance: amountconfig.distance,
    };

    myChart2 = echarts.init(chartRef2.current, {
      renderer: "canvas",
      useDirtyRect: false,
    });

    myChart2.setOption({
      series: [
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
        {
          label: labelOption,
        },
      ],
    });

    window.addEventListener("resize", myChart2.resize());
  };

  const Provide_BarchartMonth_RecycleChart = async (e) => {
    let myChart2;
    let option2;

    const month_recyclenum = JSON.stringify(specifymonth_amont);

    try {
      // 初始化图表並設定option參數顯示
      // eslint-disable-next-line no-unused-vars
      myChart2 = echarts.init(chartRef2.current, {
        renderer: "canvas",
        useDirtyRect: false,
      });

      // const series_all = recycleItem.map((name, index) => {
      //   // 確保當 `index` 不超過 `array` 的長度時，才使用 `array` 中的數值
      //   // const data = specifymonth_amont.slice(index * 5, (index + 1) * 5).map(Number); // 假設每個 `name` 有 5 個數據點

      //   const data = specifymonth_amont.split(",");

      //   return {
      //     name: name,
      //     type: "bar",
      //     label: {
      //       labelOption_Init,
      //     },
      //     emphasis: { focus: "series" },
      //     data: data[index],
      //   };
      // });

      // console.log("series_all 重整 = " + series_all);

      option2 = {
        tooltip: {
          trigger: "axis",
          axisPointer: {
            type: "shadow",
          },
        },
        legend: {
          data: [{ recycleItem }],
        },
        toolbox: {
          show: true,
          orient: "vertical",
          left: "right",
          top: "center",
          feature: {
            mark: { show: true },
            dataView: { show: true, readOnly: false },
            magicType: { show: true, type: ["line", "bar", "stack"] },
            restore: { show: true },
            saveAsImage: { show: true },
          },
        },
        xAxis: [
          {
            type: "category",
            axisTick: { show: false },
            // data: ["2012", "2013", "2014", "2015", "2016"],
            data: [
              selectedYear + "年-" + selectedMonth + "月處理量單位(公斤/月)",
            ],
          },
        ],
        yAxis: [
          {
            type: "value",
          },
        ],
        // series: [series_all],
        //將擷取的月份各資源項目解析
        series: [
          {
            name: recycleItem[0],
            type: "bar",
            barGap: 5,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[0]],
          },
          {
            name: recycleItem[1],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[1]],
          },
          {
            name: recycleItem[2],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[2]],
          },
          {
            name: recycleItem[3],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[3]],
          },
          {
            name: recycleItem[4],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[4]],
          },
          {
            name: recycleItem[5],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[5]],
          },
          {
            name: recycleItem[6],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[6]],
          },
          {
            name: recycleItem[7],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[7]],
          },
          {
            name: recycleItem[8],
            type: "bar",
            barGap: 5,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[8]],
          },
          {
            name: recycleItem[9],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[9]],
          },
          {
            name: recycleItem[10],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[10]],
          },
          {
            name: recycleItem[11],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[11]],
          },
          {
            name: recycleItem[12],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[12]],
          },
          {
            name: recycleItem[13],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[13]],
          },
          {
            name: recycleItem[14],
            type: "bar",
            barGap: 5,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[14]],
          },
          {
            name: recycleItem[15],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[15]],
          },
          {
            name: recycleItem[16],
            type: "bar",
            barGap: 0,
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[16]],
          },
          {
            name: recycleItem[17],
            type: "bar",
            label: labelOption_Init,
            emphasis: {
              focus: "series",
            },
            data: [JSON.parse(month_recyclenum)[17]],
          },
        ],
      };

      myChart2.setOption(option2);

      if (option2 && typeof option2 === "object") {
        myChart2.setOption(option2);
      }
      window.addEventListener("resize", myChart2.resize());
    } catch (error) {
      console.error("取得資料錯誤", error);
    }
  };

  const Provide_Pie_Line_RecycleChart = async (e) => {
    let myChart;
    let option;

    try {
      // 初始化图表
      myChart = echarts.init(chartRef.current, "dark", {
        renderer: "canvas",
        useDirtyRect: false,
      });

      // myChart = echarts.init(chartRef.current, "dark", {
      //   useDirtyRect: false,
      // });

      // const chartDom = document.getElementById("chartref");
      // const myChart = echarts.init(chartDom, "dark");
      // myChart = echarts.init(chartRef.current, "dark");

      // option = {
      //   title: {
      //     text: "Stacked Line",
      //   },
      //   tooltip: {
      //     trigger: "axis",
      //   },
      //   legend: {
      //     data: ["Email", "Union Ads", "Video Ads", "Direct", "Search Engine"],
      //   },
      //   grid: {
      //     left: "3%",
      //     right: "4%",
      //     bottom: "3%",
      //     containLabel: true,
      //   },
      //   toolbox: {
      //     feature: {
      //       saveAsImage: {},
      //     },
      //   },
      //   xAxis: {
      //     type: "category",
      //     boundaryGap: false,
      //     data: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
      //   },
      //   yAxis: {
      //     type: "value",
      //   },
      //   series: [
      //     {
      //       name: "Email",
      //       type: "line",
      //       stack: "Total",
      //       data: [120, 132, 101, 134, 90, 230, 210],
      //     },
      //     {
      //       name: "Union Ads",
      //       type: "line",
      //       stack: "Total",
      //       data: [220, 182, 191, 234, 290, 330, 310],
      //     },
      //     {
      //       name: "Video Ads",
      //       type: "line",
      //       stack: "Total",
      //       data: [150, 232, 201, 154, 190, 330, 410],
      //     },
      //     {
      //       name: "Direct",
      //       type: "line",
      //       stack: "Total",
      //       data: [320, 332, 301, 334, 390, 330, 320],
      //     },
      //     {
      //       name: "Search Engine",
      //       type: "line",
      //       stack: "Total",
      //       data: [820, 932, 901, 934, 1290, 1330, 1320],
      //     },
      //   ],
      // };

      option = {
        legend: {},
        tooltip: {
          trigger: "axis",
          showContent: false,
        },
        dataset: {
          // source: [
          //   ["product", "2012", "2013", "2014", "2015", "2016", "2017"],
          //   ["Milk Tea", 56.5, 82.1, 88.7, 70.1, 53.4, 85.1],
          //   ["Matcha Latte", 51.1, 51.4, 55.1, 53.3, 73.8, 68.7],
          //   ["Cheese Cocoa", 40.1, 62.2, 69.5, 36.4, 45.2, 32.5],
          //   ["Walnut Brownie", 25.2, 37.1, 41.2, 18, 33.9, 49.1],
          // ],
          source: chartsource,
        },
        xAxis: { type: "category" },
        yAxis: { gridIndex: 0 },
        grid: { top: "55%" },
        series: [
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "line",
            smooth: true,
            seriesLayoutBy: "row",
            emphasis: { focus: "series" },
          },
          {
            type: "pie",
            id: "pie",
            radius: "30%",
            center: ["50%", "25%"],
            emphasis: {
              focus: "self",
            },
            label: {
              // formatter: "{b}: {@2012} ({d}%)",
              // formatter: `{b}: {@${encode_value}} ({d}%)`,
              formatter: "{b}: {@[" + encode_value + "]} ({d}%)",
            },
            encode: {
              // itemName: "product",
              // value: "2012",
              // tooltip: "2012",
              itemName: encode_itemName,
              value: encode_value,
              tooltip: encode_value,
            },
          },
        ],
      };

      myChart.on("updateAxisPointer", function (event) {
        const xAxisInfo = event.axesInfo[0];
        if (xAxisInfo) {
          const dimension = xAxisInfo.value + 1;
          myChart.setOption({
            series: {
              id: "pie",
              label: {
                formatter: "{b}: {@[" + dimension + "]} ({d}%)",
              },
              encode: {
                value: dimension,
                tooltip: dimension,
              },
            },
          });
        }
      });
      myChart.setOption(option);

      if (option && typeof option === "object") {
        myChart.setOption(option);
      }
      window.addEventListener("resize", myChart.resize());
    } catch (error) {
      console.error("取得資料錯誤", error);
    }
  };

  const handleDrawlinkchart = async (e) => {
    const selectyear = selectedYear;
    const selectmonth = selectedMonth;

    // console.log("selectyear = " + selectyear);
    // console.log("selectmonth = " + selectmonth);

    try {
      const response = await axios.get(
        `${config.apiBaseUrl}/recycle/getall_dateinfo`,
        //"http://localhost:3009/recycle/getall_dateinfo",
        {
          params: {
            selectyear: selectyear, // 這邊搜尋年月回饋指定全月份每天的提交回收全部紀錄
            selectmonth: selectmonth,
          },
        }
      );

      const amont_totalday_productout = response.data.dayeveryamount;
      const amont_totalmonth_productout = response.data.montheveryamount;

      // console.log("全項目每天日期總累積量:", response.data.dayeveryamount); // 取得 alldata 指定年月份全項目每天日期總累積量
      // console.log("全項目每月份總累積量:", response.data.montheveryamount); // 取得 alldata2 指定年月份全項目每月份總累積量

      if (response.status === 210) {
        //toast.success(`搜尋amont_totaldayout成功.`);
        // 将字符串分割为数组并转换为浮点数
        //const dataArray = response.data.split(",").map(Number);

        // 每天數據蒐集完成後,這邊持續找尋全部回收項目指定"年月份"月總量
        //將每日全數據及每個月月數據儲存後續渲染
        setdayout_amont(amont_totalday_productout);
        setSpecifymonth_amont(amont_totalmonth_productout);
        setChecklabelOption(true);
        viewchartevent();
        // Provide_Pie_Line_RecycleChart();
      } else if (response.status === 404) {
        toast.success(`搜尋amont_totaldayout 失敗`);
      }
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  return (
    <Form>
      <div className="recyclechart_dynamic">
        <h1 style={{ textAlign: "center" }}>
          每月各天處理量動態呈現圖(包含表單)
        </h1>
        <label style={{ paddingTop: "5%" }}>
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
        <label style={{ paddingLeft: "2%" }}>
          選擇月份：
          <select
            name="monthselect"
            value={selectedMonth}
            onChange={handleChange}
          >
            {/* <option value="">請選擇</option> */}
            {months.map((month) => (
              <option key={month} value={month}>
                {month}
              </option>
            ))}
          </select>
        </label>
        {/* {selectedYear && <p>您選擇的年份是: {selectedYear}</p>} */}
        <Button
          className="button"
          variant="primary"
          onClick={handleDrawlinkchart}
        >
          顯示圖數據
        </Button>
      </div>
      <br />
      <br />

      {/*　[新增月統計數據] ----- start*/}

      <div>
        {checklabelOption && (
          <>
            <div
              ref={chartRef2}
              style={{ width: "150%", height: "380px" }}
            ></div>

            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <h3>Rolate</h3>
              <select
                value={amountconfig.rotate}
                onChange={(e) =>
                  set_AmountConfig({ ...amountconfig, rotate: e.target.value })
                }
              >
                <option value={-90}>-90度</option>
                <option value={0}>0度</option>
                <option value={90}>90度</option>
                <option value={180}>180度</option>
              </select>
            </div>

            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <h3>Align</h3>
              <select
                value={amountconfig.align}
                onChange={(e) =>
                  set_AmountConfig({ ...amountconfig, align: e.target.value })
                }
              >
                <option value="left">Left</option>
                <option value="center">Center</option>
                <option value="right">Right</option>
              </select>
            </div>

            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <h3>Vertical Align</h3>
              <select
                value={amountconfig.verticalAlign}
                onChange={(e) =>
                  set_AmountConfig({
                    ...amountconfig,
                    verticalAlign: e.target.value,
                  })
                }
              >
                <option value="top">Top</option>
                <option value="middle">Middle</option>
                <option value="bottom">Bottom</option>
              </select>
            </div>

            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <h3>Position</h3>
              <select
                value={amountconfig.position}
                onChange={(e) =>
                  set_AmountConfig({
                    ...amountconfig,
                    position: e.target.value,
                  })
                }
              >
                {posList.map((pos) => (
                  <option key={pos} value={pos}>
                    {pos}
                  </option>
                ))}
              </select>
            </div>

            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <h3>Distance</h3>
              <input
                type="number"
                min={configParameters.distance.min}
                max={configParameters.distance.max}
                value={amountconfig.distance}
                onChange={(e) =>
                  set_AmountConfig({
                    ...amountconfig,
                    distance: parseInt(e.target.value),
                  })
                }
              />
            </div>
            <br />
            <br />
            <div style={{ position: "realtive", padding: "0px 905px" }}>
              <button
                name="month_amount"
                type="button"
                style={{
                  fontSize: "15px", // 字型大小
                  color: "#007bff", // 顏色
                  textAlign: "center", // 位置
                  width: "200px",
                  position: "realtive",
                  paddingTop: "10px",
                }}
                onClick={handleChange}
              >
                調整月數據顯示
              </button>
            </div>
          </>
        )}
      </div>
      {/*　[新增月統計數據] ----- end*/}

      <div
        // id="chartref"
        ref={chartRef}
        style={{ width: "150%", height: "630px" }}
      ></div>
      <br />
      <br />
      <br />
      {showdisplayTable && <DynamicTable data={tabledataconvert} />}
    </Form>
  );
};

export default dynamicday;
