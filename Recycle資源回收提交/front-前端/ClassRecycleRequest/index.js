import React, { useState, useEffect } from "react";
import { Form, Button } from "react-bootstrap";
import axios from "axios";
import dayjs from "dayjs";
import config from "../../config";
import "./index.scss";
import { useNavigate } from "react-router-dom";

//成功提示套件
import { toast } from "react-toastify";

const recycle_itemLIST = [
  "請選擇項目",
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

const ClassRecycleRequest = () => {
  const [formData, setFormData] = useState({
    name: "", //填擔人員名稱
    submittime: dayjs().format("YYYY-MM-DDTHH:mm"),
    region: "E008正極配料區", //區域
    itemname: "請選擇項目", //項目名稱
    itemnumber: "", //項目代碼
    maketonne: "", //本日處理量(公斤/單位)
    monthtotaltonne: "", //本月已累積處理量(公斤/單位)
    cycleStatus: "持續處理中",
    question: "", //回收處理問題
    photos: null, //照片
  });

  //姓名的下拉選項
  const [options, setOptions] = useState([]);

  //回收項目下拉選項
  const [itemvalue, setItemvalue] = useState([]);
  const [monthtone, setmonthtone] = useState([]);

  // 状态管理按钮点击次数
  const [clickCount, setClickCount] = useState(0);

  // 状态管理 input 元件的显示效果
  const [isVisible, setIsVisible] = useState(false);

  //跳轉要用到的東西
  const navigate = useNavigate();

  function splitString(responseData) {
    // 假設響應數據格式為 "(abc|def)"
    // 去掉括號
    const trimmed = responseData.replace(/^\(|\)$/g, "");
    // 根據分隔符拆分字符串
    const [searchitem, currentmonth_amont] = trimmed.split("|");
    return { searchitem, currentmonth_amont };
  }

  const reset_recycle_Inputstatus = () => {
    setClickCount(0);
    setIsVisible(false);
    setmonthtone([]);
    delete formData[formData.maketonne];
    delete formData[formData.monthtotaltonne];
    return formData;
  };

  //這個是處理表單填寫;
  const handleChange = async (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });

    //填表人員
    if (name === "name") {
      if (value === "") {
        setOptions([]);
        return;
      }
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/employee/getmemberinfo`,
          // "http://localhost:3009/employee/getmemberinfo",
          {
            params: {
              query: value,
            },
          }
        );
        setOptions(response.data); // 設置員工姓名選项
      } catch (error) {
        console.error("Error fetching options:", error);
      }
    }

    //回收項目名稱
    if (name === "itemname" && value !== "請選擇項目") {
      try {
        const response = await axios.get(
          // "http://localhost:3009/recycle/itemnumber",
          `${config.apiBaseUrl}/recycle/itemnumber`,
          {
            params: {
              query: value,
            },
          }
        );
        // console.log(response.data);
        //setItemNumber(response.data); // 設置資源項目選项
        const result = splitString(response.data);
        setItemvalue(result);
        //將之前有輸入回收紀錄的狀態回到重新設置狀態
        reset_recycle_Inputstatus();

        // 同步更新 maketonne 和 monthtotaltonne 的值
        setFormData((prevFormData) => ({
          ...prevFormData,
          maketonne: "", // 重設本日處理量
          monthtotaltonne: "", // 重設本月已累積處理量
        }));
      } catch (error) {
        console.error("Error fetching options:", error);
      }
    }

    // 本日處理量
    if (name === "maketonne") {
      //當輸入空值,清除儲存鍵值
      if (value === "") {
        // setmonthtone([]);
        // delete formData[formData.maketonne];
        // delete formData[formData.monthtotaltonne];
        // return formData;
        reset_recycle_Inputstatus();
      } else {
        if (!isNaN(value) && !/^\s*$/.test(value)) {
          setmonthtone(itemvalue.currentmonth_amont);
        } else {
          toast.error("請輸入有效的數量(小數點到第3位)格式!");
        }
      }
    }
  };

  const handletonnechange = (e) => {
    setClickCount((prevCount) => {
      const newCount = prevCount + 1;

      // 每当点击次数是3的倍數时，顯示 (確認提交編號,確認提交本月累積處理量(公斤/單位))；否则都不顯示
      setIsVisible(newCount % 3 === 0);

      if (prevCount >= 3) return setClickCount(1);
      return newCount;
    });

    if (itemvalue.length === 0) {
      toast.error("請先選擇項目再輸入處理量!");
      return;
    }

    if (String(formData.maketonne) === "") {
      // console.log("formData.maketonne = " + formData.maketonne);
      toast.error("請輸入處理量,不能為空值!");
      return;
    }

    if (formData.itemnumber !== itemvalue.searchitem) {
      formData.itemnumber = itemvalue.searchitem;
      setFormData({
        ...formData,
        itemnumber: itemvalue.searchitem,
      });
      // console.log(" 最終切換 formData.itemnumber = " + formData.itemnumber);
    }

    //  if (e.target.value !== itemvalue.searchitem) {
    //    e.target.value = itemvalue.searchitem;
    //  }
    // console.log("e.target.value currentmonth_amont = " + e.target.value);
    // console.log("formData.itemnumber 回收編號最終 = " + formData.itemnumber);

    const cycleAddsum = parseFloat(formData.maketonne) + parseFloat(monthtone);

    console.log(
      "itemvalue.currentmonth_amont 尚未更新本月份公斤 = " + monthtone
    );
    console.log("formData.maketonne 今日正要提交公斤= " + formData.maketonne);
    // console.log("cycleAddsum 計算準備加總為 = " + cycleAddsum.toFixed(3));

    // 設置 formData中的(該月份累積公斤總數)
    setFormData({
      ...formData,
      monthtotaltonne: cycleAddsum.toFixed(3),
    });

    console.log(
      "formData.monthtotaltonne 最終為 = " + formData.monthtotaltonne
    );
  };

  //這個處理送出圖片
  const allowedExtensions = ["png", "jpg", "jpeg", "webp"];
  const handleFileChange = (e) => {
    const files = e.target.files;
    let newImages = [];
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const fileExtension = file.name.split(".").pop().toLowerCase();
      if (
        !allowedExtensions.includes(fileExtension) ||
        newImages.length >= 10
      ) {
        // 如果檔案副檔名不在允許的清單中，或者已經有十張圖片，拒絕上傳
        console.log("拒絕上傳：", fileExtension);
      } else {
        console.log("允許上傳：", file.name);
        newImages.push(file);
      }
    }
    // 更新 formData 中的圖片陣列
    setFormData({ ...formData, photos: newImages });
  };

  //這個處理提交事件
  const handleSubmit = async (e) => {
    e.preventDefault();

    //如果formData.maketonne 和 formData.monthtotaltonne 沒有透過選單顯示,這邊不給予提交送出
    //if (formData.maketonne === "" || formData.monthtotaltonne === "" )
    if (formData.monthtotaltonne === "" || clickCount !== 3) {
      toast.error("請按提交按鈕,有顯示項目內容後再送出!");
      //console.log("保存失敗!");
      return;
    }

    //測試用

    const formDataToSend = new FormData();
    formDataToSend.append("name", formData.name);
    formDataToSend.append("submittime", formData.submittime);
    formDataToSend.append("region", formData.region);
    formDataToSend.append("itemname", formData.itemname);
    formDataToSend.append("itemnumber", formData.itemnumber);
    formDataToSend.append("maketonne", formData.maketonne);
    formDataToSend.append("monthtotaltonne", formData.monthtotaltonne);
    formDataToSend.append("cycleStatus", formData.cycleStatus);
    formDataToSend.append("question", formData.question);
    if (formData.photos && formData.photos.length > 0) {
      formData.photos.forEach((photo) => {
        formDataToSend.append("photos", photo);
      });
    }

    console.log(formData);
    try {
      const response = await axios.post(
        `${config.apiBaseUrl}/recycle/recycle_question`,
        // "http://localhost:3009/recycle/recycle_question",
        formDataToSend,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );
      if (response.status === 201) {
        toast.success("資料保存成功");
        console.log("資料保存成功");
        navigate("/recyclelist");

        // 這裡可以加上顯示成功訊息的邏輯，例如彈出提示框或顯示在畫面上
      } else {
        toast.error("資料保存失敗");
        console.log("保存失敗!回應狀態碼:", response.status);
      }
    } catch (error) {
      console.error("Error uploading data:", error);
      toast.error("資料保存失敗");
      console.log("保存失敗!回應狀態碼:", error);
    }
  };

  const handleSelectOption = (option) => {
    setFormData({ ...formData, name: option.memberName }); //將選項自動填入
    setOptions([]); // 清空選項
  };

  return (
    <div className="classrecyclerequest">
      <Form onSubmit={handleSubmit}>
        <Form.Group>
          <Form.Label>填表人員:</Form.Label>
          <Form.Control
            type="text"
            name="name"
            value={formData.name}
            onChange={handleChange}
            placeholder="請輸入姓名"
            required
          />
        </Form.Group>
        <ul className="autocomplete-options">
          {options.slice(0, 10).map((option, index) => (
            <li key={index} onClick={() => handleSelectOption(option)}>
              {option.memberName}
            </li>
          ))}
        </ul>
        <Form.Group>
          <Form.Label>時間:</Form.Label>
          <Form.Control
            name="submittime"
            type="datetime-local"
            value={formData.submittime}
            onChange={handleChange}
          />
        </Form.Group>
        <Form.Group>
          <Form.Label>回收區域:</Form.Label>
          {/* <Form.Control
            type="text"
            name="place"
            value={formData.place}
            onChange={handleChange}
            placeholder="區域"
            required
          /> */}
          <Form.Select
            name="region"
            value={formData.region}
            onChange={handleChange}
            required
          >
            <option value="E008正極配料區">E008正極配料區</option>
            <option value="E002負極配料區">E002負極配料區</option>
            <option value="E003正極塗佈烘烤(小)">E003正極塗佈烘烤(小)</option>
            <option value="E006正極塗佈烘烤(大)">E006正極塗佈烘烤(大)</option>
            <option value="E005注液區">E005注液區</option>
          </Form.Select>
        </Form.Group>
        <br />
        <Form.Group>
          <Form.Label>
            項目編號:
            <textarea
              value={itemvalue.searchitem}
              placeholder="待選擇顯示"
              className="textarea_setting"
            />
          </Form.Label>
          <Form.Label>
            本月累積處理量(公斤/單位):
            <textarea
              value={itemvalue.currentmonth_amont}
              placeholder="待選擇顯示"
              className="textarea_setting"
            />
          </Form.Label>
          <Form.Select
            name="itemname"
            value={formData.itemname}
            onChange={handleChange}
            required
          >
            {recycle_itemLIST.map((item, index) => (
              <option key={index} value={item} hidden={index === 0}>
                {item}
              </option>
            ))}
          </Form.Select>
        </Form.Group>
        <br />
        <Form.Group>
          本日處理量(公斤/單位)
          <Form.Control
            type="text"
            name="maketonne"
            value={formData.maketonne}
            onChange={handleChange}
            placeholder="填到小數第3位"
            required
          />
          <br />
          <Button variant="primary" onClick={() => handletonnechange()}>
            提交-請按到顯示數據
          </Button>
          {/* <p>Button clicked {clickCount} times</p> */}
          <br />
          <br />
          確認提交編號
          {isVisible && (
            <Form.Control
              type="text"
              name="itemnumber"
              value={formData.itemnumber}
              // onChange={handleChange}
              //placeholder="請輸入上面提示之項目編號"
              placeholder="待本日處理量輸入完後提交顯示"
              // required
              readOnly
              style={{ backgroundColor: "#53FF53" }} // 设置背景颜色
            />
          )}
          <br />
          確認提交本月"新"累積處理量(公斤/單位)
          {isVisible && (
            <Form.Control
              type="text"
              name="monthtotaltonne"
              value={formData.monthtotaltonne}
              // onChange={handletonnechange}
              // placeholder="請在欄位任意輸入鍵2下"
              placeholder="待本日處理量輸入完後提交顯示"
              // required
              readOnly
              style={{ backgroundColor: "#F1E1FF" }} // 设置背景颜色
            />
          )}
          {/* <textarea
            value={formData.monthtotaltonne}
            placeholder="待本日處理量輸入完後顯示"
            className="textarea_setting"
            onChange={handletonnechange}
          /> */}
        </Form.Group>
        <br />
        <Form.Group>
          <Form.Label>目前狀況:</Form.Label>
          <Form.Select
            name="cycleStatus"
            value={formData.cycleStatus}
            onChange={handleChange}
          >
            {/* <option value="處理完畢">處理完畢</option> */}
            <option value="持續處理中">持續處理中</option>
            {/* <option value="未處理">未處理</option> */}
          </Form.Select>
        </Form.Group>

        <Form.Group>
          <Form.Label>處理敘述:</Form.Label>
          <Form.Control
            as="textarea"
            rows={3}
            name="question"
            value={formData.question}
            onChange={handleChange}
            placeholder="請敘述回收情形"
            required
          />
        </Form.Group>

        <Form.Group>
          <Form.Label>回收照片:</Form.Label>
          <Form.Control
            type="file"
            name="photo"
            accept=".png, .jpg, .jpeg"
            multiple
            onChange={handleFileChange}
          />
        </Form.Group>
        <Button variant="primary" type="submit" className="mt-3">
          送出
        </Button>
      </Form>
    </div>
  );
};

export default ClassRecycleRequest;
