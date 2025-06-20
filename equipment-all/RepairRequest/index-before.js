import React, { useState } from "react";
import { Form, Button } from "react-bootstrap";
import axios from "axios";
import dayjs from "dayjs";
import config from "../../config";
import "./index.scss";
import { useNavigate } from "react-router-dom";

//成功提示套件
import { toast } from "react-toastify";

const RepairRequest = () => {
  const [formData, setFormData] = useState({
    name: "", //填擔人員名稱
    time: dayjs().format("YYYY-MM-DDTHH:mm"),
    place: "2樓組裝區", //區域
    machine: "", //設備名稱
    machineStatus: "觀察中",
    question: "", //故障問題
    photos: null, //照片
  });

  //姓名的下拉選項
  const [options, setOptions] = useState([]);
  //跳轉要用到的東西
  const navigate = useNavigate();

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
  };

  // const allowedExtensions = ["png", "jpg", "jpeg", "webp"];
  // const handleFileChange = (e) => {
  //   const file = e.target.files[0];
  //   const fileExtension = file.name.split(".").pop().toLowerCase();
  //   console.log("看一下", !allowedExtensions.includes(fileExtension));
  //   if (!allowedExtensions.includes(fileExtension)) {
  //     // 如果檔案副檔名不在允許的清單中，拒絕上傳
  //     //   alert("請上傳png,jpg或jpeg檔案");
  //     console.log(fileExtension);
  //     const newdata = { ...formData, photo: e.target.files[0] };
  //     console.log(newdata);
  //   } else {
  //     console.log("你好");
  //     const newdata = { ...formData, photo: e.target.files[0] };
  //     setFormData({ ...formData, photo: e.target.files[0] });
  //     console.log(newdata);
  //   }
  // };
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
    const formDataToSend = new FormData();
    formDataToSend.append("name", formData.name);
    formDataToSend.append("time", formData.time);
    formDataToSend.append("place", formData.place);
    formDataToSend.append("machine", formData.machine);
    formDataToSend.append("machineStatus", formData.machineStatus);
    formDataToSend.append("question", formData.question);
    if (formData.photos && formData.photos.length > 0) {
      formData.photos.forEach((photo) => {
        formDataToSend.append("photos", photo);
      });
    }
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

  const handleSelectOption = (option) => {
    setFormData({ ...formData, name: option.employee_name }); //將選項自動填入
    setOptions([]); // 清空選項
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
          {/* <Form.Control
            type="text"
            name="place"
            value={formData.place}
            onChange={handleChange}
            placeholder="區域"
            required
          /> */}
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
        </Form.Group>
        <Button variant="primary" type="submit" className="mt-3">
          送出
        </Button>
      </Form>
    </div>
  );
};

export default RepairRequest;
