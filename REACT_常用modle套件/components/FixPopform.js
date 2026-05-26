import React, { useState, useEffect, useCallback, useRef } from "react";
import debounce from "lodash.debounce";
import config from "../config";
import { Form, Button } from "react-bootstrap";
import axios from "axios";
import dayjs from "dayjs";
import Table from "react-bootstrap/Table";
import { Container, Row, Col } from "react-bootstrap";
import "./repair_popform.scss";
//成功提示套件
import { toast } from "react-toastify";

const FixPopform = ({ FormMachineList, side, closeModal }) => {
  const latestQueryRef = useRef(null);
  const [visibleRows, setVisibleRows] = useState(20); // Initially show 20 rows
  const [groupedDevices, setGroupedDevices] = useState({
    一期設備: [],
    二期設備: [],
    模組設備: [],
    // 廠務設備: [],
    空調溫控類: [],
    溫控水泵塔類: [],
    空壓乾燥類: [],
    製冷裝置類: [],
    真空循環氣壓類: [],
    立式空調類: [],
    輪轉類: [],
  }); // 分組資料

  //確認FileNamelist 狀況
  useEffect(() => {
    if (side !== "Repair" && side !== "Factory") {
      toast.error("站別必須是 'Repair' 或 'Factory'");
      closeModal();
    }

    const grouped = FormMachineList.reduce(
      (acc, item) => {
        const { id, name } = parseDevice(item);
        const group = getBelongGroup(id, side);
        if (!acc[group]) {
          acc[group] = [];
        }
        acc[group].push({ id, name });
        return acc;
      },
      side === "Repair"
        ? {
            一期設備: [],
            二期設備: [],
            模組設備: [],
          }
        : {
            空調溫控類: [],
            溫控水泵塔類: [],
            空壓乾燥類: [],
            製冷裝置類: [],
            真空循環氣壓類: [],
            立式空調類: [],
            輪轉類: [],
          }
    );

    setGroupedDevices(grouped);
    // console.log("分組後的設備:", grouped);

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [FormMachineList, side]);

  //解析設備名稱與ID
  const parseDevice = (item) => {
    const [id, ...nameParts] = item.split("-");

    return {
      id,
      name: nameParts.join("-"),
    };
  };

  const toNumber = (str) => parseInt(str.replace("M", ""), 10);

  //一期範圍
  const phase1Start = toNumber("M01097");
  const phase1End = toNumber("M01707");
  const phase1_zero_start = toNumber("M00087");
  const phase1_zero_end = toNumber("M00960");
  //二期範圍
  const phase2Start = toNumber("M02079");
  const phase2End = toNumber("M003371"); // 注意順序不連續但字串仍可轉換

  //模組設備範圍
  const moduleStart = toNumber("M01173");
  const moduleEnd = toNumber("M01180");
  const modulesingle1 = toNumber("M00687");
  const modulesingle2 = toNumber("M01412");

  //廠務設備分類以下
  //空調溫控類:
  const air_control_start = toNumber("M00478");
  const air_control_end = toNumber("M00480");
  const air_outlet_control_start = toNumber("M01181");
  const air_outlet_control_end = toNumber("M01191");

  //溫控水泵塔類:
  const temprature_pump_start = toNumber("M01192");
  const temprature_pump_end = toNumber("M01206");

  //空壓乾燥類:
  const air_pressure_dry_start = toNumber("M00743");
  const air_pressure_dry_end = toNumber("M00747");
  const Freeze_adsorption_attach_start = toNumber("M01207");
  const Freeze_adsorption_attach_end = toNumber("M01215");

  //製冷裝置類:
  const refriger_device_start = toNumber("M01216");
  const refriger_device_end = toNumber("M01227");
  const ice_water_mainhost = toNumber("M00748");
  const risegift_water_mainhost = toNumber("M01236");

  //真空循環氣壓類:
  const Vacuum_circle_start = toNumber("M00467");
  const Vacuum_circle_end = toNumber("M00742");
  const Vacuum_circle_emission_start = toNumber("M01228");
  const Vacuum_circle_emission_end = toNumber("M01235");
  const Vacuum_circle_emission_2_start = toNumber("M02130");
  const Vacuum_circle_emission_2_end = toNumber("M02132");
  const Dual_Vacuum_pump_two_num = toNumber("M00468469");

  //立式空調類:
  const Vertical_aircondition_start = toNumber("M01237");
  const Vertical_aircondition_end = toNumber("M01249");

  //輪轉類:
  const tire_type_start = toNumber("M01250");
  const tire_type_end = toNumber("M01251");

  //判斷是哪一期或模組設備
  const getBelongGroup = (id, side) => {
    let num;
    // 確保 id 是字串
    const idStr = String(id);
    if (idStr.includes("~")) {
      const dual_num = id.split("~");
      const num1 = toNumber(dual_num[0]);
      const num2 = toNumber(dual_num[1]);
      // num = num1.toString() + num2.toString(); 字串比對
      num = Number(`${num1}${num2}`); //用數字比對
      // console.log(
      //   "有~ num 轉換為=" + num + " num1 = " + num1 + "num2 = " + num2
      // );
    } else {
      num = toNumber(id);
    }

    //線上設備報修站
    if (side === "Repair") {
      if (
        (num >= moduleStart && num <= moduleEnd) ||
        // num === modulesingle1 ||
        num === modulesingle2
      ) {
        return "模組設備";
      } else if (
        (num >= phase1Start && num <= phase1End) ||
        (num >= phase1_zero_start && num <= phase1_zero_end)
      ) {
        return "一期設備";
      } else if (
        (num >= phase2Start && num <= 273000) || // 處理 M02079 ~ M02730
        (num >= 300000 && num <= toNumber("M003371")) // M003370, M003371...
      ) {
        return "二期設備";
      }
    } //廠務設備報修站
    else if (side === "Factory") {
      if (
        (num >= air_control_start && num <= air_control_end) ||
        (num >= air_outlet_control_start && num <= air_outlet_control_end)
      ) {
        return "空調溫控類";
      } else if (num >= temprature_pump_start && num <= temprature_pump_end) {
        return "溫控水泵塔類";
      } else if (
        (num >= air_pressure_dry_start && num <= air_pressure_dry_end) ||
        (num >= Freeze_adsorption_attach_start &&
          num <= Freeze_adsorption_attach_end)
      ) {
        return "空壓乾燥類";
      } else if (
        (num >= refriger_device_start && num <= refriger_device_end) ||
        num === ice_water_mainhost ||
        num === risegift_water_mainhost
      ) {
        return "製冷裝置類";
      } else if (
        (num >= Vacuum_circle_start && num <= Vacuum_circle_end) ||
        (num >= Vacuum_circle_emission_start &&
          num <= Vacuum_circle_emission_end) ||
        (num >= Vacuum_circle_emission_2_start &&
          num <= Vacuum_circle_emission_2_end) ||
        num === Dual_Vacuum_pump_two_num
      ) {
        return "真空循環氣壓類";
      } else if (
        num >= Vertical_aircondition_start &&
        num <= Vertical_aircondition_end
      ) {
        return "立式空調類";
      } else if (num >= tire_type_start && num <= tire_type_end) {
        return "輪轉類";
      }
    }
  };

  const clearViewItems = (event) => {
    // 清空以下儲存內容數據
    // setRowcatch_num(0); // 若需要重設
    // setFirst_ID(0); // 若需要重設
    // setLast_ID(0); // 若需要重設
  };

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
    console.log(
      "groupedDevices changed:",
      JSON.stringify(groupedDevices, null, 2)
    );
  }, [groupedDevices]);

  const chunkArray = (array, size) => {
    const chunks = [];
    for (let i = 0; i < array.length; i += size) {
      chunks.push(array.slice(i, i + size));
    }
    return chunks;
  };

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
    closeModal();

    // 重置表單（假設你有使用 useRef 取得 form DOM）
    // formRef.current?.reset();
  };

  // Handle form submission
  const handleSubmit = (e) => {
    e.preventDefault();

    closeModal(); // Close the modal after submission
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
                  marginTop: "35px",
                  marginRight: "120px",
                  paddingTop: "25px",
                  paddingLeft: "12px",
                  fontSize: "41px",
                  fontFamily: "Bungee Spice", // ← 注意這裡要大寫 F
                  color: "#FFFF00",
                  backgroundColor: "#0000E3",
                }}
              >
                當前設備清單▼
              </h3>
              <br />

              <button
                type="button"
                style={{ marginLeft: "100px", backgroundColor: "red" }}
                onClick={handleCancel}
              >
                關閉
              </button>
            </div>
            <div
              style={{
                maxHeight: "1000px",
                overflowY: "auto",
                display: "block",
                width: "100%", // Optional, set width if needed
                paddingBottom: "1150px", // ✅ 給所有內容一個統一的底部空間
              }}
            >
              <Container fluid>
                {/* 一期 + 二期：左右欄 */}
                {side === "Repair" && (
                  <>
                    <Row>
                      <Col md={12}>
                        <h4 style={{ marginTop: "20px" }}>📦 一期設備</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["一期設備"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={12}>
                        <h4 style={{ marginTop: "20px" }}>📦 二期設備</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["二期設備"]}
                          groupSize={3}
                        />
                      </Col>
                    </Row>

                    {/* 模組設備：整排 */}
                    <Row style={{ paddingBottom: "550px" }}>
                      <Col md={12}>
                        <h4 style={{ marginTop: "40px" }}>🔧 模組設備</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["模組設備"]}
                          groupSize={4}
                        />
                      </Col>
                    </Row>
                  </>
                )}
                {side === "Factory" && (
                  <>
                    <Row>
                      <Col md={6}>
                        <h4 style={{ marginTop: "20px" }}>🌀 空調溫控類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["空調溫控類"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={6}>
                        <h4 style={{ marginTop: "20px" }}>💧 溫控水泵塔類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["溫控水泵塔類"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={6}>
                        <h4 style={{ marginTop: "100px" }}>🪫 空壓乾燥類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["空壓乾燥類"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={6}>
                        <h4 style={{ marginTop: "100px" }}>🧊 製冷裝置類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["製冷裝置類"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={6}>
                        <h4 style={{ marginTop: "100px" }}>
                          ⚙️ 真空循環氣壓類
                        </h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["真空循環氣壓類"]}
                          groupSize={3}
                        />
                      </Col>
                      <Col md={6}>
                        <h4 style={{ marginTop: "100px" }}>🌬️ 立式空調類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["立式空調類"]}
                          groupSize={3}
                        />
                      </Col>
                    </Row>
                    <Row style={{ paddingBottom: "550px" }}>
                      <Col>
                        <h4 style={{ marginTop: "40px" }}>🔁 輪轉類</h4>
                        <HorizontalDeviceTable
                          devices={groupedDevices["輪轉類"]}
                          groupSize={4}
                        />
                      </Col>
                    </Row>
                  </>
                )}
              </Container>
            </div>
          </div>
        </div>
      </form>
    </div>
  );
};

export default FixPopform;
