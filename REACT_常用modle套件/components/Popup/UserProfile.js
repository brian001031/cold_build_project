import React, { useEffect, useState } from "react";
import { Modal, Form, Button, Alert, Row, Col } from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth } from "../../context/GlobalProvider";
import axios from "axios";
import config from "../../config";
import { FormattedMessage, useIntl } from "react-intl";

const UserProfile = ({ show, onHide, centered }) => {
  const { user, login } = useAuth();
  const intl = useIntl();
  const [isLoading, setIsLoading] = useState(false);
  const [feedback, setFeedback] = useState(false);
  const [dataState, setDataState] = useState({});

  useEffect(() => {
    if (user) {
      setDataState({
        telephone: user.telephone ? user.telephone : "",
        authPosition: user.authPosition ? user.authPosition : [],
        isManager: user.isManager ? user.isManager : "0",
        memEmail: user.memEmail ? user.memEmail : "",
        reg_schedulename: user.reg_schedulename ? user.reg_schedulename : "",
        memberID: user.memberID ? user.memberID : "",
        originalpasswd: user.originalpasswd ? user.originalpasswd : "",
        positionarea: user.positionarea ? user.positionarea : "",
        shift: user.shift ? user.shift : "",
      });
    }
  }, [user]);

  const handleUpdate = async (e) => {
    e.preventDefault();

    try {
      // 修正：正確的 axios.post 格式
      const response = await axios.put(
        `${config.apiBaseUrl}/schedule/updateRegister`,
        // `http://localhost:3009/schedule/updateRegister`,
        {
          memberID: dataState.memberID,
          name: dataState.reg_schedulename,
          telephone: dataState.telephone,
          memEmail: dataState.memEmail,
          originalpasswd: dataState.originalpasswd,
          positionarea: Number(user.authStatus) > 0 ? dataState.positionarea : [],
          shift: dataState.shift,
          authPosition: Number(user.authStatus) > 0 ? dataState.authPosition : [],
        },
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      console.log("Registration response:", response.data);

      if (response.status === 200) {
        setFeedback(true);
        toast.success("資料更新成功！");
        login(dataState);
        onHide(); // 關閉彈窗
        // 重置表單
      }

      // toast.success('註冊成功！');
    } catch (error) {
      console.error("Registration error:", error);
      if (error.response) {
        // 服務器返回錯誤
        if (error.response.status === 403) {
          toast.error("此工號已經註冊過了");
        } else {
          const errorMessage =
            error.response.data?.message || "更新失敗，請檢查輸入資料";
          toast.error(errorMessage);
        }
      } else if (error.request) {
        // 網路錯誤
        toast.error("網路連線失敗，請確認連線狀況");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    onHide();
  };

  return (
    <Modal
      show={show}
      onHide={handleClose}
      centered={centered}
      size="md"
      backdrop="static"
    >
      <Modal.Header closeButton className="bg-primary text-white">
        <Modal.Title
          style={{
            display: "flex",
            fontSize: "3rem",
            fontWeight: "bold",
            color: "white",
            justifyContent: "center",
            width: "100%",
          }}
        >
          <i className="bi bi-person-circle me-2"></i>
          <FormattedMessage id="Update.Uptitle" defaultMessage="資料更新" />
        </Modal.Title>
      </Modal.Header>

      <Modal.Body className="px-4 py-4">
        {/* 註冊表單 */}
        <Form onSubmit={handleUpdate}>
          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold w-100"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-person me-2"></i>
              <FormattedMessage id="Login.id" defaultMessage="工號" />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder={intl.formatMessage({
                id: "Regis.erqlempl_ID",
                defaultMessage: "請輸入工號",
              })}
              value={dataState.memberID}
              onChange={(e) =>
                setDataState({ ...dataState, memberID: e.target.value.trim() })
              }
              size="lg"
              disabled
              required
              autoFocus
            />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold w-100"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-person me-2"></i>
              <FormattedMessage id="Update.name" defaultMessage="姓名" />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder="請輸入工號"
              value={dataState.reg_schedulename}
              onChange={(e) =>
                setDataState({ ...dataState, memberID: e.target.value.trim() })
              }
              size="lg"
              disabled
              required
              autoFocus
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold w-100"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-envelope me-2"></i>
              <FormattedMessage
                id="Regis.phonenumber"
                defaultMessage="電話號碼"
              />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder={intl.formatMessage({
                id: "Regis.phonenumber_entry",
                defaultMessage: "請輸入電話號碼 , ex: 0912345678",
              })}
              value={dataState.telephone}
              onChange={(e) =>
                setDataState({ ...dataState, telephone: e.target.value.trim() })
              }
              size="lg"
              disabled={isLoading}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold w-100"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-envelope me-2"></i>
              <FormattedMessage id="Regis.email" defaultMessage="電子郵箱" />
            </Form.Label>
            <Form.Control
              type="email"
              placeholder={intl.formatMessage({
                id: "Regis.req_email",
                defaultMessage: "請輸入電子郵箱",
              })}
              value={dataState.memEmail}
              onChange={(e) =>
                setDataState({ ...dataState, memEmail: e.target.value.trim() })
              }
              size="lg"
              disabled={isLoading}
            />
          </Form.Group>

          <Form.Group className="mb-4">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-lock me-2"></i>
              <FormattedMessage id="Regis.pwd" defaultMessage="密碼 *" />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder={intl.formatMessage({
                id: "Regis.req_password",
                defaultMessage: "請輸入密碼",
              })}
              value={dataState.originalpasswd}
              onChange={(e) =>
                setDataState({ ...dataState, originalpasswd: e.target.value })
              }
              size="lg"
              disabled={isLoading}
              required
              minLength={3}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-geo-alt me-2"></i>
              <FormattedMessage id="Regis.workarea" defaultMessage="工作區域" />
            </Form.Label>
            <Form.Select
              value={dataState.positionarea}
              onChange={(e) =>
                setDataState({ ...dataState, positionarea: e.target.value })
              }
              size="lg"
              disabled
            >
              <option value="">
                {" "}
                {intl.formatMessage({
                  id: "Regis.req_work_area",
                  defaultMessage: "請選擇工作區域",
                })}
              </option>
              <option value="混漿區">混漿區|Slurry mixing area</option>
              <option value="塗佈區">塗佈區|Coating area</option>
              <option value="輾壓區">輾壓區|Rolling area</option>
              <option value="電芯組裝區">電芯組裝區|Cell assembly area </option>
              <option value="電化學區">電化學區|Electrochemical area </option>
              <option value="模組組裝區">
                模組組裝區|Module assembly area{" "}
              </option>
              <option value="產品組裝區">
                產品組裝區|Product assembly area{" "}
              </option>
              <option value="模組與產品測試區">
                模組與產品測試區|Module and product testing area
              </option>
            </Form.Select>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-clock me-2"></i>
              <FormattedMessage id="Regis.depart" defaultMessage="所屬部門" />
            </Form.Label>
            <Form.Select
              value={dataState.authPosition}
              onChange={(e) =>
                setDataState({ ...dataState, authPosition: e.target.value })
              }
              size="lg"
              disabled
            >
              {dataState.authPosition ? (
                <option value={dataState.authPosition}>
                  {dataState.authPosition}
                </option>
              ) : (
                <option value="">請選擇部門</option>
              )}
            </Form.Select>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-clock me-2"></i>
              <FormattedMessage id="Regis.shift" defaultMessage="班別" />
            </Form.Label>
            <Form.Select
              value={dataState.shift}
              onChange={(e) =>
                setDataState({ ...dataState, shift: e.target.value })
              }
              size="lg"
              disabled
            >
              <option value="">請選擇班別</option>
              <option value="早班">早班</option>
              <option value="晚班">晚班</option>
              <option value="常日班">常日班</option>
            </Form.Select>
          </Form.Group>

          {/* 分隔線 */}
          <div className="text-center mb-4">
            <hr
              style={{
                border: "none",
                borderTop: "2px solid #dee2e6",
                margin: "0 20%",
              }}
            />
          </div>

          {/* 按鈕區域 */}
          <Row className="g-2">
            <Col xs={6}>
              <Button
                variant="secondary"
                size="lg"
                className="w-100"
                onClick={handleClose}
                disabled={isLoading}
              >
                <FormattedMessage id="Regis.cancel" defaultMessage="取消" />
              </Button>
            </Col>
            <Col xs={6}>
              <Button
                type="submit"
                variant="primary"
                size="lg"
                className="w-100"
                disabled={isLoading}
              >
                {isLoading ? (
                  <>
                    <span
                      className="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    更新中...
                  </>
                ) : (
                  <>
                    <i className="bi bi-person-plus me-2"></i>
                    <FormattedMessage
                      id="Update.up_string"
                      defaultMessage="更新"
                    />
                  </>
                )}
              </Button>
            </Col>
          </Row>
        </Form>
      </Modal.Body>
    </Modal>
  );
};

export default UserProfile;
