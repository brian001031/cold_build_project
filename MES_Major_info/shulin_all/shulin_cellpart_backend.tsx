import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { shulin_cellpart_backend_box } from "../../data";
import config from "../../styles/config";
import axios from "axios";
import "./shulin_cellpart_backend.scss";

//---------------後段cell-電芯-性能耐壓處理序(3)----------------------------
function shulin_cellpart_backend() {
  const apiMesUrl = import.meta.env.VITE_MES_API_URL;
  const [mesdata, setmesdata] = useState([]);
  const [currentdata, setcurrentData] = useState(""); // 存放字串數據
  const [previousData, setPreviousData] = useState(""); // 儲存上次獲取的字串
  const [reflahCount, setreflahCount] = useState(0);
  const [isUpdated, setIsUpdated] = useState(false); // 用來標示字串是否變更
  const [progress, setProgress] = useState(0); // 進度條的進度
  const isValid = true; // 外部連結布林值,預設啟動true

  const [chemos_cap_preid, setchemos_cap_Preid] = useState("");
  const [chemos_cap_preid2, setchemos_cap_Preid2] = useState("");

  const [chemos_cap_SecondCC1, setchemos_cap_SecondCC1] = useState("");
  const [chemos_cap_SecondCC2, setchemos_cap_SecondCC2] = useState("");

  const [RT_cap_stroge1, setRT_Aging_stroge1] = useState("");
  const [RT_cap_stroge2, setRT_Aging_stroge2] = useState("");

  let mesmap_backend: Map<unknown, unknown>;

  const [currentbackenddata, setcurrentbackenddata] = useState<{
    [key: string]: string;
  }>({
    chmos_1: "",
    chmos_2: "",
    cap_1: "",
    cap_2: "",
    ht_ag: "",
    rt_ag1: "",
    rt_ag2: "",
    edge1: "",
    edge2: "",
    sulting: "",
  });

  const [previousbackenddata, setpreviousbackenddata] = useState<{
    [key: string]: string;
  }>({
    previous_chmos_1: "",
    previous_chmos_2: "",
    previous_cap_1: "",
    previous_cap_2: "",
    previous_ht_ag: "",
    previous_rt_ag1: "",
    previous_rt_ag2: "",
    previous_edge1: "",
    previous_edge2: "",
    previous_sulting: "",
  });

  const keymatch = [
    "(最新工作序號)",
    "(設備數量[線上/總])",
    "(生產人員[線上/總])",
    "(生產工單)",
    "(本日產能)",
  ];

  const keymatch_HT_AT_Aging = [
    "(最新工作序號)",
    "(設備數量[線上/總])",
    "(生產人員[線上/總])",
    "(生產工單)",
    "(本日產能)",
    "(在庫數量)",
    "(溫度)",
  ];

  const backend_site_info = [
    "chmos_1",
    "chmos_2",
    "cap_1",
    "cap_2",
    "ht_ag",
    "rt_ag1",
    "rt_ag2",
    "edge1",
    "edge2",
    "sulting",
  ];

  const is_rt_backend_all_key = [
    "is_rt_chmos_1",
    "is_rt_chmos_2",
    "is_rt_cap_1",
    "is_rt_cap_2",
    "is_rt_ht_ag",
    "is_rt_rt_ag1",
    "is_rt_rt_ag2",
    "is_rt_edge1",
    "is_rt_edge2",
    "is_rt_sulting",
  ];

  let mesmap_part1: Map<unknown, unknown>;
  const [CellMap_backend, setCellMap_backend] = useState<[unknown, unknown][]>(
    []
  );

  // 用來儲存字串的陣列，每個字串會有一個 "updated" 標記，來表示是否更新
  const [data, setData] = useState<{ text: string; updated: boolean }[]>([]);
  const [dataMap, setDataMap] = useState<{
    [key in (typeof keymatch)[number]]: string;
  }>({});

  const [isbackendupdate, setisbackendupdate] = useState<{
    [key: string]: boolean; // 這裡使用 string 作為鍵類型，boolean 作為值類型
  }>({
    is_rt_chmos_1: false,
    is_rt_chmos_2: false,
    is_rt_cap_1: false,
    is_rt_cap_2: false,
    is_rt_ht_ag: false,
    is_rt_rt_ag1: false,
    is_rt_rt_ag2: false,
    is_rt_edge1: false,
    is_rt_edge2: false,
    is_rt_sulting: false,
  });

  //更新最新ID
  const updateID = (key: string, value: string) => {
    setcurrentbackenddata((prevData: any) => ({
      ...prevData, // 保持其他值不變
      [key]: value, // 更新指定的 key
    }));
  };

  //儲存上次已經更新的ID
  const storageID = (key: string, value: string) => {
    setpreviousbackenddata((prevData: any) => ({
      ...prevData, // 保持其他值不變
      [key]: value, // 更新指定的 key
    }));
  };

  const station_backend_update = (key: string, isupdate: boolean) => {
    setisbackendupdate((prevState: any) => ({
      ...prevState, // 保持其他值不變
      [key]: isupdate, // 更新指定的 key
    }));
  };

  const clear_AllBackendUpdate_default = (keys: string[]) => {
    setisbackendupdate((prevState) => {
      const fixfalsestatus = { ...prevState }; // 複製之前的狀態
      keys.forEach((key) => {
        fixfalsestatus[key] = false; // 更新每個 key為ALL false
      });
      return fixfalsestatus; // 正確返回更新後的狀態
    });
  };

  useEffect(() => {
    let backend_splitselfItem_final: React.SetStateAction<
      Map<unknown, unknown>[]
    >;

    // 使用 fetch 請求後端 API
    const timer = setInterval(() => {
      const fetchData_WO = async () => {
        try {
          const response = await axios.get(
            // `${apiMesUrl}/mes/cellpart_backend`
            // `${config.apiBaseUrl}/mes/cellpart_backend`
            "http://localhost:3009/mes/cellpart_backend"
          );

          const woresult = await response.data;
          setCellMap_backend([]); //清除所有map數據

          // console.log("woresult 收到總= " + woresult);

          const backend_splitselfItem = woresult.map((item: string) =>
            item.split(",")
          );

          //將後段全站realtime資訊收集
          for (let run = 0; run < backend_splitselfItem.length; run++) {
            const backend_station = backend_splitselfItem[run];
            let chemos_pfId1: React.SetStateAction<any>;
            let chemos_pfId2: React.SetStateAction<any>;
            let chemos_CC_ONEId1: React.SetStateAction<any>;
            let chemos_CC_TWOId2: React.SetStateAction<any>;
            let RT_Aging_ONEId1: React.SetStateAction<any>;
            let RT_Aging_TWOId2: React.SetStateAction<any>;

            //化成分容站點(占用2站),後站取第5個位元起始, 所以是要從後第2位開始索引
            const keyupdate_HT = backend_site_info[run + 2];
            // let keyupdate_edge = backend_site_info[run + 3];
            const keyupdate_sult = backend_site_info[run + 4];

            const array_backend = backend_station.map((item: any) =>
              item.split("|")
            );

            //當只有化成,分容,常溫倉站ID需要做以下處理
            if (run === 0 || run === 1 || run === 3) {
              const responseData = array_backend[0];
              let splitdevide = responseData[0].split(",");
              const testfilter = splitdevide[0]
                .replace(/\s/g, "")
                .replace("N/A", "")
                .split("/");

              //化成站
              if (run === 0) {
                chemos_pfId1 = testfilter[0].toString().split(" ");
                chemos_pfId2 = testfilter[1].toString().split(" ");
                setchemos_cap_Preid(chemos_pfId1);
                setchemos_cap_Preid2(chemos_pfId2);
              } //分容站
              else if (run === 1) {
                chemos_CC_ONEId1 = testfilter[0].toString().split(" ");
                chemos_CC_TWOId2 = testfilter[1].toString().split(" ");

                setchemos_cap_SecondCC1(chemos_CC_ONEId1);
                setchemos_cap_SecondCC2(chemos_CC_TWOId2);
              } //常溫倉站
              else if (run === 3) {
                RT_Aging_ONEId1 = testfilter[0].toString().split(" ");
                RT_Aging_TWOId2 = testfilter[1].toString().split(" ");

                setRT_Aging_stroge1(RT_Aging_ONEId1);
                setRT_Aging_stroge2(RT_Aging_TWOId2);
              }

              // if (parseInt(chemos_pfId1).toString().includes("7003")) {
              //   console.log("找到顯示一期ID為= " + chemos_pfId1);
              // } else {
              //   console.log("找無一期ID為= " + chemos_pfId1);
              // }

              // if (parseInt(chemos_pfId2).toString().includes("5260")) {
              //   console.log("找到顯示二期ID為= " + chemos_pfId2);
              // } else {
              //   console.log("找無二期ID為= " + chemos_pfId2);
              // }
            }

            backend_splitselfItem_final = array_backend[0].map(
              (item: string, index: any) => {
                //更新站點ID
                if (run === 0) {
                  if (index === 0) {
                    // //化成站 PF一期
                    updateID(backend_site_info[0], chemos_pfId1);

                    // //化成站 PF二期
                    updateID(backend_site_info[1], chemos_pfId2);
                  }
                } else if (run === 1) {
                  if (index === 0) {
                    //亂數1~1000提供測試用-----------start----------------
                    // const randnum = Math.floor(Math.random() * 1000) + 1;
                    // console.log("隨機數字= " + randnum);
                    // if (randnum % 2) {
                    //   updateID(
                    //     backend_site_info[2],
                    //     chemos_CC_ONEId1 + randnum.toString()
                    //   );
                    // } else {
                    //   //分容站 CC2 一期
                    //   updateID(
                    //     backend_site_info[3],
                    //     chemos_CC_TWOId2 + randnum.toString()
                    //   );
                    // }
                    //提供測試用-----------end----------------
                    //分容站 CC1 一期
                    updateID(backend_site_info[2], chemos_CC_ONEId1);
                    // // //分容站 CC2 一期
                    updateID(backend_site_info[3], chemos_CC_TWOId2);
                  }
                } else if (run === 3) {
                  if (index === 0) {
                    // const randnum = Math.floor(Math.random() * 1000) + 1;
                    //常溫倉站 一期
                    updateID(backend_site_info[5], RT_Aging_ONEId1);
                    //常溫倉站 二期
                    updateID(backend_site_info[6], RT_Aging_TWOId2);
                  }
                }
                //當站點只有一期會往這邊進行ID新判斷
                //高溫倉
                else if (run === 2) {
                  if (index === 0) {
                    updateID(keyupdate_HT, item);
                  }
                }
                // run=4:精密封裝 , run=5:選判
                if (run > 3) {
                  if (run === 4 && index === 0) {
                    //走訪2次,因為有一二期
                    for (let k = 1; k < 3; k++) {
                      let edge_triggerID: React.SetStateAction<any>;
                      //因為有一二期所以要往後索引2位▽
                      const keyupdate_edge = backend_site_info[run + k + 2];
                      let edgeinfo = item.match(new RegExp(`Edge${k}:(\\d+)`));
                      edge_triggerID = edgeinfo ? edgeinfo[1] : null; // 取得EdgeX:後數字部分

                      // if (k === 1) {
                      //   console.log("精封站一期ID= " + edge_triggerID);
                      // } else {
                      //   console.log("精封站二期ID= " + edge_triggerID);
                      // }

                      updateID(keyupdate_edge, edge_triggerID);
                    }
                  } else if (run === 5 && index === 0) {
                    updateID(keyupdate_sult, item);
                  }
                }
              }
            );

            //高常溫站有多顯示數據(在庫數量,溫度)
            if (run === 2 || run === 3) {
              mesmap_backend = new Map(
                array_backend[0].map((item: { name: any }, index: any) => [
                  keymatch_HT_AT_Aging[index],
                  item,
                ]) // 使用keymatch陣列的索引作為 key，item 作為 value
              );
            } else {
              mesmap_backend = new Map(
                array_backend[0].map((item: { name: any }, index: any) => [
                  keymatch[index],
                  item,
                ]) // 使用keymatch陣列的索引作為 key，item 作為 value
              );
            }

            const arrayFromMap: [unknown, unknown][] =
              Array.from(mesmap_backend);

            setCellMap_backend((prev: any) => [...prev, ...arrayFromMap]);
          }

          // 將 Map 轉換為陣列
          // const arrayFromMap: [unknown, unknown][] = Array.from(mesmap_part1);

          // 將轉換後的陣列設置到 state 中
          // setCellMap_backend(arrayFromMap);

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
    //currentdata 數據value偵測是否有變化

    // console.log("當下chmos_1 = " + currentbackenddata.chmos_1);
    // console.log("之前chmos_1 = " + previousbackenddata.previous_chmos_1);

    // console.log("當下chmos_2 = " + currentbackenddata.chmos_2);
    // console.log("之前chmos_2 = " + previousbackenddata.previous_chmos_2);

    // console.log("當下capcc1_1 = " + currentbackenddata.cap_1);
    // console.log("之前capcc1_1 = " + previousbackenddata.previous_cap_1);

    // console.log("當下capcc2_1 = " + currentbackenddata.cap_2);
    // console.log("之前 capcc2_1 = " + previousbackenddata.previous_cap_2);

    //化成機台PF一期
    if (currentbackenddata.chmos_1 !== previousbackenddata.previous_chmos_1) {
      let chmos_currentID = currentbackenddata.chmos_1
        .toString()
        .replace(/[^0-9]/g, "");
      let chmos_previousID = previousbackenddata.previous_chmos_1
        .toString()
        .replace(/[^0-9]/g, "");

      //當搜尋有數字不一致時,判定更新觸發
      if (parseInt(chmos_currentID) !== parseInt(chmos_previousID)) {
        storageID("previous_chmos_1", currentbackenddata.chmos_1); // 儲已更新存的字串
        station_backend_update("is_rt_chmos_1", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("化成機台PF一期更新進行中!");
      }
    }
    //化成機台PF二期
    if (currentbackenddata.chmos_2 !== previousbackenddata.previous_chmos_2) {
      const chmos_current_chmos2ID = currentbackenddata.chmos_2
        .toString()
        .replace(/[^0-9]/g, "");
      const chmos_previous_chmos2ID = previousbackenddata.previous_chmos_2
        .toString()
        .replace(/[^0-9]/g, "");

      //當搜尋有數字不一致時,判定更新觸發
      if (
        parseInt(chmos_current_chmos2ID) !== parseInt(chmos_previous_chmos2ID)
      ) {
        storageID("previous_chmos_2", currentbackenddata.chmos_2); // 儲已更新存的字串
        station_backend_update("is_rt_chmos_2", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("化成機台PF二期更新進行中!");
      }
    }
    //分容機台CC1一期
    if (currentbackenddata.cap_1 !== previousbackenddata.previous_cap_1) {
      const chmos_current_cap1ID = currentbackenddata.cap_1
        .toString()
        .replace(/[^0-9]/g, "");
      const chmos_previous_cap1ID = previousbackenddata.previous_cap_1
        .toString()
        .replace(/[^0-9]/g, "");
      if (parseInt(chmos_current_cap1ID) !== parseInt(chmos_previous_cap1ID)) {
        storageID("previous_cap_1", currentbackenddata.cap_1); // 儲已更新存的字串
        station_backend_update("is_rt_cap_1", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("分容機台CC1一期更新進行中!");
      }
    }
    //分容機台CC2一期
    if (currentbackenddata.cap_2 !== previousbackenddata.previous_cap_2) {
      const chmos_current_cap2ID = currentbackenddata.cap_2
        .toString()
        .replace(/[^0-9]/g, "");
      const chmos_previous_cap2ID = previousbackenddata.previous_cap_2
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(chmos_current_cap2ID) !== parseInt(chmos_previous_cap2ID)) {
        storageID("previous_cap_2", currentbackenddata.cap_2); // 儲已更新存的字串
        station_backend_update("is_rt_cap_2", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("分容機台CC2一期更新進行中!");
      }
    }
    //H.T. Aging(高溫倉靜置)
    if (currentbackenddata.ht_ag !== previousbackenddata.previous_ht_ag) {
      const ht_ag_current_ID = currentbackenddata.ht_ag
        .toString()
        .replace(/[^0-9]/g, "");
      const ht_ag_previous_ID = previousbackenddata.previous_ht_ag
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(ht_ag_current_ID) !== parseInt(ht_ag_previous_ID)) {
        storageID("previous_ht_ag", currentbackenddata.ht_ag); // 儲已更新存的字串
        station_backend_update("is_rt_ht_ag", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("H.T. Aging高溫倉更新進行中!");
      }
    }

    //R.T. Aging(常溫倉靜置)一期
    if (currentbackenddata.rt_ag1 !== previousbackenddata.previous_rt_ag1) {
      const rt_ag1_current_ID = currentbackenddata.rt_ag1
        .toString()
        .replace(/[^0-9]/g, "");
      const rt_ag1_previous_ID = previousbackenddata.previous_rt_ag1
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(rt_ag1_current_ID) !== parseInt(rt_ag1_previous_ID)) {
        storageID("previous_rt_ag1", currentbackenddata.rt_ag1); // 儲已更新存的字串
        station_backend_update("is_rt_rt_ag1", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("R.T. Aging常溫倉一期更新進行中!");
      }
    }

    //R.T. Aging(常溫倉靜置)二期
    if (currentbackenddata.rt_ag2 !== previousbackenddata.previous_rt_ag2) {
      const rt_ag2_current_ID = currentbackenddata.rt_ag2
        .toString()
        .replace(/[^0-9]/g, "");
      const rt_ag2_previous_ID = previousbackenddata.previous_rt_ag2
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(rt_ag2_current_ID) !== parseInt(rt_ag2_previous_ID)) {
        storageID("previous_rt_ag2", currentbackenddata.rt_ag2); // 儲已更新存的字串
        station_backend_update("is_rt_rt_ag2", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("R.T. Aging常溫倉二期更新進行中!");
      }
    }

    //Edge精密封裝一期
    if (currentbackenddata.edge1 !== previousbackenddata.previous_edge1) {
      const edge1_current_ID = currentbackenddata.edge1
        .toString()
        .replace(/[^0-9]/g, "");
      const edge1_previous_ID = previousbackenddata.previous_edge1
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(edge1_current_ID) !== parseInt(edge1_previous_ID)) {
        storageID("previous_edge1", currentbackenddata.edge1); // 儲已更新存的字串
        station_backend_update("is_rt_edge1", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("精封站一期更新進行中!");
      }
    }

    //Edge精密封裝二期
    if (currentbackenddata.edge2 !== previousbackenddata.previous_edge2) {
      const edge2_current_ID = currentbackenddata.edge2
        .toString()
        .replace(/[^0-9]/g, "");
      const edge2_previous_ID = previousbackenddata.previous_edge2
        .toString()
        .replace(/[^0-9]/g, "");

      if (parseInt(edge2_current_ID) !== parseInt(edge2_previous_ID)) {
        storageID("previous_edge2", currentbackenddata.edge2); // 儲已更新存的字串
        station_backend_update("is_rt_edge2", true);
        setIsUpdated(true); // 標記數據更新
        setProgress(0); // 重置進度條
        console.log("精封站二期更新進行中!");
      }
    }
  }, [currentbackenddata, previousbackenddata]); //  當 currentdata 和 previousbackenddata改變時觸發

  // 進度條更新效果
  useEffect(() => {
    if (isUpdated) {
      const timer = setInterval(() => {
        setProgress((prev) => {
          if (prev >= 100) {
            clearInterval(timer);
            setIsUpdated(false); // 更新完成後停止動畫
            clear_AllBackendUpdate_default(is_rt_backend_all_key);
            return 100;
          }
          return prev + 20;
        });
      }, 500);
      return () => clearInterval(timer);
    }
  }, [isUpdated]);

  return (
    <div className="shulin_cellpart_backend">
      {shulin_cellpart_backend_box.map((item, indexbackend) => (
        <div className={`box box${item.id}`}>
          <div className="image-container">
            <a
              href={isValid ? `${item.url}${item.optionkey}` : "#"}
              target="_blank"
              rel="noopener noreferrer"
            >
              <img className="image" src={item.icon} alt=" "></img>{" "}
            </a>
            {/* 动态链接 */}
          </div>
          <span className="title">{item.title}</span>
          {/* <span className="productvalue">{item.equipment_qty}</span>
          <span className="productvalue">{item.op_num}</span>
          <span className="productvalue">{item.workorder}</span> */}
          <span className="productvalue">{item.currentday_capacity}</span>
          <ul>
            {/* 化成 */}
            {indexbackend == 0 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 0 && index <= 4 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color:
                        isbackendupdate.is_rt_chmos_1 ||
                        isbackendupdate.is_rt_chmos_2
                          ? "#EA0000"
                          : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
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

                    {index >= 3 && index <= 4 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}
                    {index === 0 && (
                      <div>
                        <div>{chemos_cap_preid + "/" + chemos_cap_preid2}</div>
                      </div>
                    )}
                  </span>
                </span>
              ))}

            {/* 分容 */}
            {indexbackend == 1 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 5 && index <= 9 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color:
                        isbackendupdate.is_rt_cap_1 ||
                        isbackendupdate.is_rt_cap_2
                          ? "#EA0000"
                          : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
                    {index === 6 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index === 7 && (
                      <div>
                        <div>
                          {(value as React.ReactNode) && value + "/" + value}
                        </div>
                      </div>
                    )}

                    {index >= 8 && index <= 9 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}
                    {index === 5 && (
                      <div>
                        <div>
                          {chemos_cap_SecondCC1 + "/" + chemos_cap_SecondCC2}
                        </div>
                      </div>
                    )}
                  </span>
                </span>
              ))}

            {/* 高溫倉 */}
            {indexbackend == 2 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 10 && index <= 16 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color: isbackendupdate.is_rt_ht_ag
                        ? "#EA0000"
                        : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
                    {index === 11 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index === 12 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {(index >= 13 && index <= 16 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )) ||
                      (index === 10 && (
                        <div>
                          <div>{value as React.ReactNode}</div>
                        </div>
                      ))}
                  </span>
                </span>
              ))}

            {/*中溫倉 */}
            {indexbackend == 3 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 17 && index <= 23 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color:
                        isbackendupdate.is_rt_rt_ag1 ||
                        isbackendupdate.is_rt_rt_ag2
                          ? "#EA0000"
                          : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
                    {index === 18 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index === 19 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index >= 20 && index <= 23 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}
                    {index === 17 && (
                      <div>
                        <div>{RT_cap_stroge1 + "/" + RT_cap_stroge2}</div>
                      </div>
                    )}
                  </span>
                </span>
              ))}

            {/* 晶封裝 */}
            {indexbackend == 4 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 24 && index <= 28 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color:
                        isbackendupdate.is_rt_edge1 ||
                        isbackendupdate.is_rt_edge2
                          ? "#EA0000"
                          : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
                    {index === 25 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index === 26 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {(index >= 27 && index <= 28 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )) ||
                      (index === 24 && (
                        <div>
                          <div>{value as React.ReactNode}</div>
                        </div>
                      ))}
                  </span>
                </span>
              ))}

            {/* 分選判別 */}
            {indexbackend == 5 &&
              CellMap_backend.map(([key, value], index) => (
                <span
                  key={index}
                  style={{
                    fontSize: "20",
                    color: "#2828FF",
                    display: "block",
                  }}
                >
                  {index >= 29 && index <= 33 && (key as React.ReactNode)}
                  <span
                    style={{
                      fontSize: "20",
                      display: "block",
                      fontWeight: "bold",
                      color: isbackendupdate.is_rt_sulting
                        ? "#EA0000"
                        : "#FFFF37",
                      transition: "color 0.3s ease-in-out",
                    }}
                  >
                    {index === 30 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {index === 31 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )}

                    {(index >= 32 && index <= 33 && (
                      <div>
                        <div>{value as React.ReactNode}</div>
                      </div>
                    )) ||
                      (index === 29 && (
                        <div>
                          <div>{value as React.ReactNode}</div>
                        </div>
                      ))}
                  </span>
                </span>
              ))}
          </ul>
          {/* {Object.entries(mesmap_part1).map((value, index) => (
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
          //marginBottom: "20px",
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
          }}
        >
          {isUpdated && (
            <span
              style={{
                fontSize: "110px",
                color: "#EA0000",
                display: "inline-block",
                textAlign: "end",
                position: "absolute",
                paddingLeft: "100px",
              }}
            >
              {reflahCount === 1
                ? "全站"
                : isbackendupdate.is_rt_chmos_1
                ? "化成一期"
                : isbackendupdate.is_rt_chmos_2
                ? "化成二期"
                : isbackendupdate.is_rt_cap_1
                ? "分容CC1"
                : isbackendupdate.is_rt_cap_2
                ? "分容CC2"
                : isbackendupdate.is_rt_ht_ag
                ? "高溫倉"
                : isbackendupdate.is_rt_rt_ag1
                ? "中溫倉一期"
                : isbackendupdate.is_rt_rt_ag2
                ? "中溫倉二期"
                : isbackendupdate.is_rt_edge1
                ? "精封一期"
                : isbackendupdate.is_rt_edge2
                ? "精封二期"
                : isbackendupdate.is_rt_sulting
                ? "分選判別"
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

export default shulin_cellpart_backend;
