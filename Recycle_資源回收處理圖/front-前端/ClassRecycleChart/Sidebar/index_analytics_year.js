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

const flags = [
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

const updateFrequency = 3000;
const dimension = 2;
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
    flags.find(function (item) {
      return item.name === cycle_itemname;
    }) || {}
  ).emoji;
}

const Analyticsyear = () => {
  const chartRef = useRef(null); // 创建 ref 来引用 DOM 元素
  const currentYear = new Date().getFullYear();
  const years = [];

  // 生成從至今到2年前的年分
  for (let i = 0; i >= -2; i--) {
    years.push(currentYear + i);
  }

  const [selectedYear, setSelectedYear] = useState("");
  const [chartyearamont, setchartyearamont] = useState([]);

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
          //"http://localhost:3009/recycle/getyearamont",
          {
            params: {
              year: currentYear,
            },
          }
        );

        console.log(response.data);
        if (response.status === 200) {
          // 将字符串分割为数组并转换为浮点数
          const dataArray = response.data.split(",").map(Number);
          setchartyearamont(dataArray);
        }

        // 初始化图表
        myChart = echarts.init(chartRef.current);

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
            axisPointer: {
              type: "cross",
              textStyle: {
                align: "left",
              },
            },
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
            data: [
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
            ],
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
              seriesLayoutBy: "column",
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
          graphic: echarts.util.map(chartyearamont, function (item, dataIndex) {
            return {
              type: "text",
              right: 160,
              bottom: 1,
              style: {
                text: selectedYear,
                font: "bolder 80px monospace",
                fill: "rgba(100, 100, 100, 0.25)",
              },
              z: 100,
              onmousemove: echarts.util.curry(showTooltip, dataIndex),
              onmouseout: echarts.util.curry(hideTooltip, dataIndex),
            };
          }),
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

    // 清理工作：组件卸载时销毁图表实例
    return () => {
      if (!option) {
        myChart.dispose();
      }
    };
  }, []);

  useEffect(() => {
    if (chartyearamont) {
      afterReflashRecycleChart();
    } else {
      setchartyearamont([]);
    }
  }, [chartyearamont]);

  const afterReflashRecycleChart = async (e) => {
    let myChart;

    try {
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
          data: [
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
          ],
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
            seriesLayoutBy: "column",
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
        graphic: echarts.util.map(chartyearamont, function (item, dataIndex) {
          return {
            type: "text",
            right: 160,
            bottom: 1,
            style: {
              text: selectedYear,
              font: "bolder 80px monospace",
              fill: "rgba(100, 100, 100, 0.25)",
            },
            z: 100,
            onmousemove: echarts.util.curry(showTooltip, dataIndex),
            onmouseout: echarts.util.curry(hideTooltip, dataIndex),
          };
        }),
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
          //"http://localhost:3009/recycle/getyearamont",
          {
            params: {
              year: value,
            },
          }
        );

        // console.log(response.data);
        if (response.status === 200) {
          // 将字符串分割为数组并转换为浮点数
          const dataArray = response.data.split(",").map(Number);
          setchartyearamont(dataArray);
          afterReflashRecycleChart();
        }
      } catch (error) {
        console.error("Error fetching selectyear:", error);
      }
    }
  };

  return (
    <Form>
      <div>
        <h1>回收年數據分析</h1>
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
      <div ref={chartRef} style={{ width: "100%", height: "600px" }}></div>
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
