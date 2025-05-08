import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { shulin_cellpart_middle_box } from "../../data";
import config from "../../styles/config";
import axios from "axios";
import "./shulin_cellpart_middle.scss";
import { InputType } from "zlib";
import { parse } from "path/win32";

//---------------中段cell-電芯-電機化加工處理序(2)----------------------------
function shulin_cellpart_middle() {
  const apiMesUrl = import.meta.env.VITE_MES_API_URL;
  const [mesdata, setmesdata] = useState([]);
  const [currentdata, setcurrentData] = useState(""); // 存放字串數據
  const [previousData, setPreviousData] = useState(""); // 儲存上次獲取的字串
  const [reflahCount, setreflahCount] = useState(0);
  const [currentmiddledata, setcurrentmiddledata] = useState<{
    [key: string]: string;
  }>({
    mc_cathode: "",
    mc_anode: "",
    z_fol: "",
    stack: "",
    oven: "",
    filling: "",
  });

  const [previousmiddledata, setpreviousmiddledata] = useState<{
    [key: string]: string;
  }>({
    previous_mc_cathode: "",
    previous_mc_anode: "",
    previous_z_fol: "",
    previous_stack: "",
    previous_oven: "",
    previous_filling: "",
  });

  const middle_site_info = [
    "mc_cathode",
    "mc_anode",
    "z_fol",
    "stack",
    "oven",
    "filling",
  ];

  const [isUpdated, setIsUpdated] = useState(false); // 用來標示字串是否變更
  const [progress, setProgress] = useState(0); // 進度條的進度
  const isValid = true; // 外部連結布林值,預設啟動true

  const keymatch = [
    "(最新工作序號)",
    "(設備數量[線上/總])",
    "(生產人員[線上/總])",
    "(生產工單)",
    "(本日產能)",
  ];

  let mesmap_middle: Map<unknown, unknown>;
  const [CellMap_middle, setCellMap_middle] = useState<[unknown, unknown][]>(
    []
  );

  const [CellMap_middleData, setCellMap_middleData] = useState<
    Map<unknown, unknown>[]
  >([]);

  // 用來儲存字串的陣列，每個字串會有一個 "updated" 標記，來表示是否更新
  const [data, setData] = useState<{ text: string; updated: boolean }[]>([]);
  const [dataMap, setDataMap] = useState<{
    [key in (typeof keymatch)[number]]: string;
  }>({});

  const is_rt_all_key = [
    "is_rt_mc_cathode",
    "is_rt_mc_anode",
    "is_rt_z_fol",
    "is_rt_stack",
    "is_rt_oven",
    "is_rt_filling",
  ];

  const [ismiddleupdate, setismiddleupdate] = useState<{
    [key: string]: boolean; // 這裡使用 string 作為鍵類型，boolean 作為值類型
  }>({
    is_rt_mc_cathode: false,
    is_rt_mc_anode: false,
    is_rt_z_fol: false,
    is_rt_stack: false,
    is_rt_oven: false,
    is_rt_filling: false,
  });

  function groupcell_product(array: [any], subgouplength: any) {
    let index = 0;
    let refixarray = [];
    while (index < array.length) {
      refixarray.push(array.slice(index, (index += subgouplength)));
    }
    return refixarray;
  }

  const clear_AllMiddleUpdate_default = (keys: string[]) => {
    setismiddleupdate((prevState) => {
      const fixfalsestatus = { ...prevState }; // 複製之前的狀態
      keys.forEach((key) => {
        fixfalsestatus[key] = false; // 更新每個 key
      });
      return fixfalsestatus; // 正確返回更新後的狀態
    });
  };

  //更新最新ID
  const updateID = (key: string, value: string) => {
    setcurrentmiddledata((prevData: any) => ({
      ...prevData, // 保持其他值不變
      [key]: value, // 更新指定的 key
    }));
  };

  //儲存上次已經更新的ID
  const storageID = (key: string, value: string) => {
    setpreviousmiddledata((prevData: any) => ({
      ...prevData, // 保持其他值不變
      [key]: value, // 更新指定的 key
    }));
  };

  const station_middle_update = (key: string, isupdate: boolean) => {
    setismiddleupdate((prevState: any) => ({
      ...prevState, // 保持其他值不變
      [key]: isupdate, // 更新指定的 key
    }));
  };

  useEffect(() => {
    // 使用 fetch 請求後端 API

    const timer = setInterval(() => {
      let middle_splitselfItem_final: React.SetStateAction<
        Map<unknown, unknown>[]
      >;
      // let datamap: Map<unknown, unknown>; // 创建一个新的 Map
      const fetchData_WO = async () => {
        try {
          const response = await axios.get(
            // `${apiMesUrl}/mes/cellpart_middle`
            // `${config.apiBaseUrl}/mes/cellpart_middle`
            "http://localhost:3009/mes/cellpart_middle"
          );

          const woresult = await response.data;

          setCellMap_middle([]); //清除所有map數據

          //console.log(woresult);
          // const woresult2 = groupcell_product(await response.data, 5);

          //判斷本日產能是否有更新

          const middle_splitselfItem = woresult.map((item: string) =>
            item.split(",")
          );

          //將中段全站realtime資訊收集
          for (let run = 0; run < middle_splitselfItem.length; run++) {
            const middle_station = middle_splitselfItem[run];

            const keyupdate = middle_site_info[run];

            const array_middle = middle_station.map((item: any) =>
              item.split("|")
            );

            middle_splitselfItem_final = array_middle[0].map(
              (item: string, index: any) => {
                //更新站點ID
                {
                  index === 0 && updateID(keyupdate, item);
                }

                // console.log(index + keyupdate[index] + " 物件： " + item);
                // const map = new Map<string, string>();
                // map.set(keymatch[index], item);
                // return { id: index + 1, map }; // 返回每個 Map 和對應的 id
              }
            );

            // datamap = new Map();
            // array_middle[0].map((item: string, index: any) => {
            //   if (keymatch[index]) {
            //     // 确保 keymatch 数组有对应的键
            //     datamap.set(keymatch[index], item); // 使用 keymatch[index] 作为键，item 作为值
            //   }
            // });

            mesmap_middle = new Map(
              array_middle[0].map((item: { name: any }, index: any) => [
                keymatch[index],
                item,
              ]) // 使用keymatch陣列的索引作為 key，item 作為 value
            );

            const arrayFromMap: [unknown, unknown][] =
              Array.from(mesmap_middle);

            setCellMap_middle((prev: any) => [...prev, ...arrayFromMap]);

            // console.log("run=" + run + " , " + JSON.stringify(mesmap_middle));

            // array_middle[0].forEach((item: any, idx: number) => {
            //   map.set(`column_${idx + 1}`, item); // 用 column_1, column_2... 作為 keymatch
            // });
          }

          // const map =
          //   // 使用 reduce() 將值與 key 結合，並消除隱式的 any 類型
          //   values.reduce(
          //     (acc: { [x: string]: any }, value: any, index: string | number) => {
          //       acc[key[index]] = value;
          //       return acc;
          //     },
          //     {} as { [key in (typeof keymatch)[number]]: string }
          //   ); // 明確指定類型

          // setDataMap(map);
        } catch (error) {
          console.error("Error fetching WO data:", error);
        }
      };

      fetchData_WO();
      setreflahCount((prevCount) => {
        const newCount = prevCount + 1;
        return newCount;
      });
    }, 3000); // 每3秒鐘执行一次(1000毫秒X3)
    return () => clearInterval(timer); // 清除計時器
  }, []);

  useEffect(() => {
    // console.log("CellMap_middleData 目前狀態為= " + CellMap_middleData);
  }, [CellMap_middle]);

  useEffect(() => {
    //currentdata 數據value偵測是否有變化

    // console.log("currentmiddledata.filling = " + currentmiddledata.filling);
    // console.log(
    //   "previousmiddledata.previous_filling = " +
    //     previousmiddledata.previous_filling
    // );

    //入殼機
    if (currentmiddledata.stack !== previousmiddledata.previous_stack) {
      storageID("previous_stack", currentmiddledata.stack); // 儲已更新存的字串
      station_middle_update("is_rt_stack", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("入殼機最新更新進行中!");
    } //注液機
    if (currentmiddledata.filling !== previousmiddledata.previous_filling) {
      storageID("previous_filling", currentmiddledata.filling); // 儲已更新存的字串
      station_middle_update("is_rt_filling", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("注液機最新更新進行中!");
    } // 疊片機
    if (currentmiddledata.z_fol !== previousmiddledata.previous_z_fol) {
      storageID("previous_z_fol", currentmiddledata.z_fol); // 儲已更新存的字串
      station_middle_update("is_rt_z_fol", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("疊片機最新更新進行中!");
    }

    //正極模切機
    if (
      currentmiddledata.mc_cathode !== previousmiddledata.previous_mc_cathode
    ) {
      storageID("previous_mc_cathode", currentmiddledata.mc_cathode); // 儲已更新存的字串
      station_middle_update("is_rt_mc_cathode", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("正極模切機最新更新進行中!");
    }
    //負極模切機
    if (currentmiddledata.mc_anode !== previousmiddledata.previous_mc_anode) {
      storageID("previous_mc_anode", currentmiddledata.mc_anode); // 儲已更新存的字串
      station_middle_update("is_rt_mc_anode", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("負極模切機最新更新進行中!");
    }

    //(電芯-大烘箱/極片-小烘箱)
    if (currentmiddledata.oven !== previousmiddledata.previous_oven) {
      storageID("previous_oven", currentmiddledata.oven); // 儲已更新存的字串
      station_middle_update("is_rt_oven", true);
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
      //console.log("真空烘烤最新更新進行中!");
    }
  }, [currentmiddledata, previousmiddledata]); // 當  改變時觸發

  useEffect(() => {
    //currentdata 數據value偵測是否有變化
    if (currentdata !== previousData) {
      setPreviousData(currentdata); // 更新已儲存的字串
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
    }
  }, [currentdata, previousData]); // 當 currentdata 改變時觸發

  // 進度條更新效果
  useEffect(() => {
    if (isUpdated) {
      const timer = setInterval(() => {
        setProgress((prev: number) => {
          if (prev >= 100) {
            clearInterval(timer);
            setIsUpdated(false); // 更新完成後停止動畫
            clear_AllMiddleUpdate_default(is_rt_all_key);
            // console.log(ismiddleupdate);
            return 100;
          }
          return prev + 20;
        });
      }, 500);
      return () => clearInterval(timer);
    }
  }, [isUpdated]);

  return (
    <div className="shulin_cellpart_middle">
      {shulin_cellpart_middle_box.map((item, indexmiddle) => (
        <div className={`box box${item.id}`}>
          <div className="image-container">
            <a
              href={isValid ? `${item.url}${item.optionkey}` : "#"}
              target="_blank"
              rel="noopener noreferrer"
            >
              <img className="image" src={item.icon} alt=" "></img>
            </a>
          </div>
          <span className="title">{item.title}</span>
          {/* <span className="productvalue">{item.equipment_qty}</span>
          <span className="productvalue">{item.op_num}</span>
          <span className="productvalue">{item.workorder}</span> */}
          <span className="productvalue">{item.currentday_capacity}</span>

          {/* 正極模切 */}
          {indexmiddle === 0 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_mc_cathode
                      ? "#FF5809"
                      : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {/* {"middle box 框號:" + indexmiddle} */}
                  {index >= 0 && index <= 4 && (key as React.ReactNode)}
                  {/* {index >= 15 &&
                    index <= 19 &&
                    (value as React.ReactNode) &&
                    value + "/" + value} */}
                  {/* {"<-序號:" + index} */}

                  {index === 1 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {index === 2 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 3 && index <= 4) || index === 0) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* 負極模切 */}
          {indexmiddle === 1 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_mc_anode
                      ? "#FF5809"
                      : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {/* {"middle box 框號:" + indexmiddle} */}
                  {index >= 5 && index <= 9 && (key as React.ReactNode)}
                  {/* {index >= 15 &&
                    index <= 19 &&
                    (value as React.ReactNode) &&
                    value + "/" + value} */}
                  {/* {"<-序號:" + index} */}

                  {index === 6 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {index === 7 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 8 && index <= 9) || index === 5) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* Z疊片 */}
          {indexmiddle === 2 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_z_fol ? "#FF5809" : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {/* {"middle box 框號:" + indexmiddle} */}
                  {index >= 10 && index <= 14 && (key as React.ReactNode)}
                  {/* {index >= 15 &&
                    index <= 19 &&
                    (value as React.ReactNode) &&
                    value + "/" + value} */}
                  {/* {"<-序號:" + index} */}

                  {index === 11 && <div>{value as React.ReactNode}</div>}
                  {index === 12 && (
                    <div>
                      <div>1 /{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 13 && index <= 14) || index === 10) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* 入殼機 */}
          {indexmiddle === 3 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                {/* {key as React.ReactNode} */}
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_stack ? "#FF5809" : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {index >= 15 && index <= 19 && (key as React.ReactNode)}
                  {index === 16 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {index === 17 && (
                    <div>
                      <div>1 /{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 18 && index <= 19) || index === 15) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* 電芯-大烘箱/極片-小烘箱 */}
          {indexmiddle === 4 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_oven ? "#FF5809" : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {/* {"middle box 框號:" + indexmiddle} */}
                  {index >= 20 && index <= 24 && (key as React.ReactNode)}
                  {/* {index >= 15 &&
                    index <= 19 &&
                    (value as React.ReactNode) &&
                    value + "/" + value} */}
                  {/* {"<-序號:" + index} */}

                  {index === 21 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {index === 22 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 23 && index <= 24) || index === 20) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* 注液機 */}
          {indexmiddle === 5 &&
            CellMap_middle.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#AE0000",
                  display: "block",
                }}
              >
                {/* {key as React.ReactNode} */}
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: ismiddleupdate.is_rt_filling ? "#FF5809" : "#0000E3",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {index >= 25 && index <= 29 && (key as React.ReactNode)}
                  {index === 26 && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                  {index === 27 && (
                    <div>
                      <div>1 /{value as React.ReactNode}</div>
                    </div>
                  )}
                  {((index >= 28 && index <= 29) || index === 25) && (
                    <div>
                      <div>{value as React.ReactNode}</div>
                    </div>
                  )}
                </span>
              </span>
            ))}

          {/* {Object.entries(mesmap_middle).map((value, index) => (
            <span
              style={{
                fontSize: "20",
                color: "#75",
                display: "block",
              }}
            >
              {value[0]}
              <span
                style={{
                  fontSize: "20",
                  color: "#FFD306",
                  display: "block",
                }}
              >
                {""}
                {value[1]}
              </span>
            </span>
          ))}
          ; */}
        </div>
      ))}
      {/* {Object.entries(dataMap).map((value, index) => (
        <span style={{ fontSize: "25px", color: "#750075" }}>
          {value[0]}
          {"-"}
          {value[1]}
        </span>
      ))} */}
      {/* <pre>{JSON.stringify(dataMap, null, 2)}</pre>; */}

      <div
        style={{
          paddingBottom: "150px",
          width: "100%",
          backgroundColor: "#f0Ff0",
          borderRadius: "10x",
        }}
      >
        <div
          style={{
            height: "20px",
            width: `${progress}%`,
            backgroundColor: progress === 100 ? "green" : "blue",
            borderRadius: "10px",
            transition: "width 0.5s ease-in-out",
            position: "relative",
            marginRight: "300px",
          }}
        >
          {isUpdated && (
            <span
              style={{
                fontSize: "110px",
                color: "#EA0000",
                display: "inline-block",
                textAlign: "end",
                position: "relative",
                paddingTop: "20px",
                // paddingBottom: "300px",
                // paddingLeft: "100px ",
              }}
            >
              {reflahCount === 1
                ? "全站"
                : ismiddleupdate.is_rt_stack
                ? "入殼站"
                : ismiddleupdate.is_rt_filling
                ? "注液站"
                : ismiddleupdate.is_rt_z_fol
                ? "疊片站"
                : ismiddleupdate.is_rt_mc_cathode
                ? "正極模切站"
                : ismiddleupdate.is_rt_mc_anode
                ? "負極模切站"
                : ismiddleupdate.is_rt_oven
                ? "真空烘烤站"
                : ""}
              更新中
            </span>
          )}
        </div>
      </div>
      <span className="span_shulintext">樹 林 廠</span>
    </div>
  );
}

export default shulin_cellpart_middle;
