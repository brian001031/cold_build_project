import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import config from "../../config";
import "./index.scss";
import { Card, Form, Button, Image } from "react-bootstrap";
import dayjs from "dayjs";
import { toast } from "react-toastify";
import PhotoCarousel from "../../components/PhotoCarousel";
import Photo_FlexTemplate from "../../components/ImageFlexDisplay";

const RepairEdit = () => {
  const { id } = useParams();
  const [formData, setFormData] = useState(null);

  //姓名的下拉選項
  const [options, setOptions] = useState([]);

  const handleChange = async (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });

    if (name === "repair_person") {
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

  //照片部分
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
  // const handleSubmit = async (e) => {
  //   e.preventDefault();
  //   console.log(formData);
  // };
  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log(formData);
    // 創建 FormData 對象，用於將表單數據發送到後端
    const formDataToSend = new FormData();
    formDataToSend.append("machine", formData.machine);
    formDataToSend.append("handled", formData.handled);
    formDataToSend.append("repair_person", formData.repair_person);
    formDataToSend.append("handling_method", formData.handling_method);
    formDataToSend.append("confirmation_method", formData.confirmation_method);
    formDataToSend.append("reoperation_time", formData.reoperation_time);
    if (formData.photos && formData.photos.length > 0) {
      formData.photos.forEach((photo) => {
        formDataToSend.append("photos", photo);
      });
    }

    try {
      // 使用 axios.patch 方法將表單數據發送到後端進行更新
      const response = await axios.patch(
        `${config.apiBaseUrl}/repair/repair_list/${id}`,
        formDataToSend,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );

      // 檢查 response 的狀態碼，如果狀態碼為 201，表示更新成功
      if (response.status === 200) {
        toast.success("資料保存成功");
        console.log("資料保存成功");
        window.location.reload();
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
    setFormData({ ...formData, repair_person: option.employee_name }); //將選項自動填入
    setOptions([]); // 清空選項
  };

  useEffect(() => {
    const fetchRepairItem = async () => {
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/repair/repair_list/${id}`
        );
        const data = response.data[0];
        if (!data.reoperation_time) {
          // 如果 reoperation_time 為 null，則設置為當前時間
          data.reoperation_time = dayjs().format("YYYY-MM-DDTHH:mm");
        }
        console.log(data);
        setFormData(data);
      } catch (error) {
        console.error("取得資料錯誤", error);
      }
    };

    fetchRepairItem();
  }, [id]);
  return (
    <>
      {formData && (
        <Form onSubmit={handleSubmit}>
          <Card style={{ marginBottom: "10px", backgroundColor: "#DDD" }}>
            <Card.Body>
              <Card.Title>故障問題</Card.Title>
              <div>
                <Form.Group>
                  <Form.Label>報修人員:</Form.Label>
                  <Form.Control
                    type="text"
                    name="name"
                    value={formData.name}
                    onChange={handleChange}
                    placeholder="報修人員"
                    disabled
                  />
                </Form.Group>
                <Form.Group>
                  <Form.Label>報修時間:</Form.Label>
                  <Form.Control
                    type="datetime-local"
                    value={dayjs(formData.time).format("YYYY-MM-DDTHH:mm")}
                    onChange={handleChange}
                    disabled
                  />
                </Form.Group>
                <Form.Group>
                  <Form.Label>區域:</Form.Label>
                  <Form.Control
                    type="text"
                    name="place"
                    value={formData.place}
                    onChange={handleChange}
                    placeholder="區域"
                    disabled
                  />
                </Form.Group>
                <Form.Group>
                  <Form.Label>故障設備:</Form.Label>
                  <Form.Control
                    type="text"
                    name="machine"
                    value={formData.machine}
                    onChange={handleChange}
                    placeholder="請輸入故障設備名稱+機台編號"
                    disabled
                  />
                  {/* {formData.photo_path && (
                    <Image
                      src={`${config.apiBaseUrl}/uploads/${formData.photo_path}`}
                      fluid
                      style={{ maxWidth: "300px", height: "auto" }}
                      className="mt-3"
                    />
                  )} */}
                  {/* {formData.photo_path &&
                    formData.photo_path.map((photo, index) => (
                      <Image
                        key={index}
                        src={`${config.apiBaseUrl}/uploads/${photo}`} // 使用 photo 變數建構圖片路徑
                        fluid
                        style={{ maxWidth: "300px", height: "auto" }}
                        className="mt-3"
                      />
                    ))} */}

                  {formData.photo_path && formData.photo_path.length === 1 ? (
                    <Image
                      src={`${config.apiBaseUrl}/uploads/${formData.photo_path[0]}`}
                      style={{ maxWidth: "300px", height: "auto" }}
                      className="mt-3"
                      alt="single-photo"
                    />
                  ) : formData.photo_path && formData.photo_path.length > 1 ? (
                    // <PhotoCarousel photoPaths={formData.photo_path} />
                    <Photo_FlexTemplate photoPaths={formData.photo_path} />
                    
                  ) : (
                    <></>
                  )}
                </Form.Group>
                <Form.Group>
                  <Form.Label>目前狀況:</Form.Label>
                  <Form.Select
                    name="machineStatus"
                    value={formData.machineStatus}
                    onChange={handleChange}
                    disabled
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
                    disabled
                  />
                </Form.Group>
              </div>
            </Card.Body>
          </Card>
          <Card className="repairEdit">
            <Card.Body>
              <Card.Title>修復回報</Card.Title>
              <div>
                <Form.Group>
                  <Form.Label>維修人:</Form.Label>
                  <Form.Control
                    type="text"
                    name="repair_person"
                    value={formData.repair_person || ""}
                    onChange={handleChange}
                    placeholder="維修人姓名"
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
                  <Form.Label>處理方式:</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={9}
                    name="handling_method"
                    value={formData.handling_method || ""}
                    onChange={handleChange}
                    placeholder="處理方式"
                    required
                  />
                </Form.Group>
                <br />
                <Form.Group>
                  <Form.Label>修復確認方式:</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={9}
                    name="confirmation_method"
                    value={formData.confirmation_method || ""}
                    onChange={handleChange}
                    placeholder="確認方法"
                    required
                  />
                </Form.Group>
                <br />
                <Form.Group>
                  <Form.Label>修復結果</Form.Label>
                  <Form.Select
                    name="handled"
                    value={formData.handled}
                    onChange={handleChange}
                  >
                    <option value="0">未修復</option>
                    <option value="2">觀察中</option>
                    <option value="1">已修復</option>
                  </Form.Select>
                </Form.Group>
                <br />
                <Form.Group>
                  <Form.Label>再作業時間:</Form.Label>
                  <Form.Control
                    type="datetime-local"
                    name="reoperation_time"
                    value={dayjs(formData.reoperation_time).format(
                      "YYYY-MM-DDTHH:mm"
                    )}
                    onChange={handleChange}
                    required
                  />
                </Form.Group>
                <Form.Group>
                  <Form.Label>修復照片:</Form.Label>
                  <Form.Control
                    type="file"
                    name="photo"
                    accept=".png, .jpg, .jpeg"
                    multiple
                    onChange={handleFileChange}
                  />
                </Form.Group>
                {formData.repair_photo && formData.repair_photo.length === 1 ? (
                  <Image
                    src={`${config.apiBaseUrl}/uploads/${formData.repair_photo[0]}`}
                    style={{ maxWidth: "300px", height: "auto" }}
                    className="mt-3"
                    alt="single-photo"
                  />
                ) : formData.repair_photo &&
                  formData.repair_photo.length > 1 ? (
                  // <PhotoCarousel photoPaths={formData.repair_photo} />
                  <Photo_FlexTemplate photoPaths={formData.repair_photo} />                  
                ) : (
                  <></>
                )}
              </div>
            </Card.Body>
          </Card>
          {/* 上面都是故報回報!!!!!!!!!!! */}

          <Button variant="primary" type="submit" className="mt-3">
            送出
          </Button>
        </Form>
      )}
    </>
  );
};

export default RepairEdit;
