import React, { useState } from "react";
import { Form, Button } from "react-bootstrap";
import axios from "axios";
import dayjs from "dayjs";
import config from "../../config";
import "./index.scss";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const RepairRequest = () => {
  const [formData, setFormData] = useState({
    name: "", //填擔人員名稱
    time: dayjs().format("YYYY-MM-DDTHH:mm"),
    place: "2樓組裝區", //區域
    machine: "", //設備名稱
    errorcode: "", //設備錯誤代碼
    machineStatus: "觀察中",
    question: "", //故障問題
    photos: null, //照片
    photoInfo: [],
  });

  //目前錯誤碼列舉,持續新增新定義狀態
  const errcode_option = [
    "E006",
    "I015.2",
    "M3210",
    "M3200",
    "M0000",
    "M3152",
    "M02091",
    "MR8304",
    "MR6805",
    "M3136",
    "MR7112",
    "M01125",
    "M0307",
    "M1006",
    "NA0",
    "R2109",
    "00c",
  ];

  //姓名的下拉選項
  const [options, setOptions] = useState([]);
  //跳轉要用到的東西
  const navigate = useNavigate();

  // 數有幾個檔案
  const [fileCount, setFileCount] = useState(0);

  //用來儲存經過篩選的選項，只顯示與輸入關鍵字匹配的選項
  const [filteredErrorOptions, setFilteredErrorOptions] = useState([]);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  //判斷錯誤碼輸入有效與否
  const [isErrCodeValid, setIsErrCodeValid] = useState(false);
  // 正規表達式判斷是否包含英文和數字
  const regex_repairerr = /^(?=.*[a-zA-Z])(?=.*\d).+$/;

  //這個是處理表單填寫
  const handleChange = async (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
    if (name === "name") {
      if (value === "") {
        setOptions([]);
        return;
      }
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/employee/getEmployeesByIDOrName`,
          {
            params: {
              query: value,
            },
          }
        );
        setOptions(response.data); // 設置選项
      } catch (error) {
        console.error("Error fetching options:", error);
      }
    }

    //判定報修設備錯誤代碼有無輸入格式錯誤(En+數字)
    if (name === "errorcode") {
      const error_str = value;
      if (error_str.length >= 1 || value !== "") {
        // 篩選錯誤碼選項
        const filtered = errcode_option.filter((option) =>
          option.toLowerCase().includes(error_str.toLowerCase())
        );
        setFilteredErrorOptions(filtered);
        setIsDropdownOpen(true);

        //取errorcode當下輸入狀態有錯誤以下可能
        if (!regex_repairerr.test(error_str)) {
          setIsErrCodeValid(false);
        } else {
          setIsErrCodeValid(true);
        }
      } else {
        setFilteredErrorOptions([]);
        setIsDropdownOpen(false);
      }

      // console.log("Error錯誤碼比對狀態:" + regex_repairerr.test(value));
    }
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
    // 更新上傳的檔案數量
    setFileCount(newImages.length);
  };

  //這個處理提交事件
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!isErrCodeValid) {
      toast.error("錯誤碼需要包含(英文+數字)!");
      return;
    }

    const formDataToSend = new FormData();
    formDataToSend.append("name", formData.name);
    formDataToSend.append("time", formData.time);
    formDataToSend.append("place", formData.place);
    formDataToSend.append("machine", formData.machine);
    formDataToSend.append("errorcode", formData.errorcode);
    formDataToSend.append("machineStatus", formData.machineStatus);
    formDataToSend.append("question", formData.question);
    if (formData.photos && formData.photos.length > 0) {
      formData.photos.forEach((photo) => {
        formDataToSend.append("photos", photo);
      });
    }
    formData.photoInfo.forEach((info, index) => {
      formDataToSend.append(`photoInfo[${index}]`, info);
    });
    console.log(formData);
    try {
      const response = await axios.post(
        `${config.apiBaseUrl}/repair/repair_question`,
        //"http://localhost:3009/repair/repair_question",
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
        navigate("/repairList");

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

  const handleFixInfoChange = (e, index) => {
    const { value } = e.target;

    if (value === 0 || undefined || "") {
      return "未填寫資料";
    }
    const newPhotoInfo = [...formData.photoInfo];
    newPhotoInfo[index] = value;
    setFormData({ ...formData, photoInfo: newPhotoInfo });
  };

  const handleSelectOption = (option) => {
    setFormData({ ...formData, name: option.employee_name }); //將選項自動填入
    setOptions([]); // 清空選項
  };

  const ErrCodeOptionSelect = (errstatus) => {
    //取errorcode當下輸入狀態有錯誤以下可能
    if (!regex_repairerr.test(errstatus)) {
      setIsErrCodeValid(false);
    } else {
      setIsErrCodeValid(true);
    }

    setFormData((prevFormData) => ({
      ...prevFormData,
      errorcode: "", // 重設設備錯誤碼
    }));
    setFormData({ ...formData, errorcode: errstatus });
    setFilteredErrorOptions([]); // 清空錯誤碼選項
    setIsDropdownOpen(false);
  };

  return (
    <div className="repairRequest">
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
              {option.employee_name}
            </li>
          ))}
        </ul>
        <Form.Group>
          <Form.Label>時間:</Form.Label>
          <Form.Control
            name="time"
            type="datetime-local"
            value={formData.time}
            onChange={handleChange}
          />
        </Form.Group>
        <Form.Group>
          <Form.Label>區域:</Form.Label>
          <Form.Select
            name="place"
            value={formData.place}
            onChange={handleChange}
            required
          >
            <option value="1樓負極塗佈區">1樓負極塗佈區</option>
            <option value="1樓正極塗佈區">1樓正極塗佈區</option>
            <option value="1樓分條區">1樓分條區</option>
            <option value="1樓碾壓區">1樓碾壓區</option>
            <option value="1樓出貨區">1樓出貨區</option>
            <option value="1樓模組區">1樓模組區</option>
            <option value="1樓電氣區">1樓電氣區</option>
            <option value="2樓組裝區">2樓組裝區</option>
            <option value="2樓模組區">2樓模組區</option>
            <option value="2樓注液區">2樓注液區</option>
            <option value="2樓電化學區">2樓電化學區</option>
            <option value="2樓混漿區">2樓混漿區</option>
            <option value="3樓研發電化學區">3樓研發電化學區</option>
            <option value="3樓研發手套箱區">3樓研發手套箱區</option>
            <option value="3樓研發工作區">3樓研發工作區</option>
            <option value="3樓落粉區">3樓落粉區</option>
          </Form.Select>
        </Form.Group>
        <Form.Group>
          <Form.Label>故障設備:</Form.Label>
          <Form.Control
            type="text"
            name="machine"
            value={formData.machine}
            onChange={handleChange}
            placeholder="請輸入故障設備名稱+機台編號"
            required
          />
        </Form.Group>
        <Form.Group>
          <Form.Label>錯誤代碼:</Form.Label>
          <Form.Control
            type="text"
            name="errorcode"
            value={formData.errorcode}
            onChange={handleChange}
            placeholder="請輸入設備錯誤代碼(英文+數字)"
            required
          />
          {isDropdownOpen && filteredErrorOptions.length > 0 && (
            <ul>
              {filteredErrorOptions.map((option, index) => (
                <li
                  key={index}
                  onClick={() => ErrCodeOptionSelect(option)}
                  className="errcode_option"
                >
                  {option}
                </li>
              ))}
            </ul>
          )}
        </Form.Group>
        <Form.Group>
          <Form.Label>目前狀況:</Form.Label>
          <Form.Select
            name="machineStatus"
            value={formData.machineStatus}
            onChange={handleChange}
          >
            <option value="觀察中">觀察中</option>
            <option value="停機">停機</option>
          </Form.Select>
        </Form.Group>

        <Form.Group>
          <Form.Label>問題敘述:</Form.Label>
          <Form.Control
            as="textarea"
            rows={3}
            name="question"
            value={formData.question}
            onChange={handleChange}
            placeholder="請敘述故障情形"
            required
          />
        </Form.Group>

        <Form.Group>
          <Form.Label>故障照片:</Form.Label>
          <Form.Control
            type="file"
            name="photo"
            accept=".png, .jpg, .jpeg"
            multiple
            onChange={handleFileChange}
          />
          <div
            style={{
              marginTop: "0.6rem",
            }}
          >
            圖片備註:
          </div>
          {Array.from({ length: fileCount }, (_, i) => (
            <input
              key={i}
              className="textArea"
              onChange={(e) => handleFixInfoChange(e, i)}
            />
          ))}
        </Form.Group>
        <Button variant="primary" type="submit" className="mt-3">
          送出
        </Button>
      </Form>
    </div>
  );
};

export default RepairRequest;
