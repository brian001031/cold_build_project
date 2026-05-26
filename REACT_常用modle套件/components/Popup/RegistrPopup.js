import React, { useEffect, useState } from "react";
import { Modal, Form, Button, Alert, Row, Col } from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth } from "../../context/GlobalProvider";
import axios from "axios";
import config from "../../config";
import { group_leave_Bulletin_New } from "../../mes_remak_data";
import { FormattedMessage, useIntl } from "react-intl";

const RegistrPopup = ({ show, onHide, centered }) => {
  // 修正：正確獲取 useAuth 的返回值
  const { user, login } = useAuth();
  const intl = useIntl();
  const [isLoading, setIsLoading] = useState(false);
  const [feedback, setFeedback] = useState(false);

  const [leaveGroups, setLeaveGroups] = useState([]);
  const [leavePos , setLeavePos] = useState([]);

  // 接收現在有的動態authPosition 選項
  useEffect(() => {
  async function fetchData() {
    const {dataSend , data_Pos} = await group_leave_Bulletin_New();

    setLeaveGroups(dataSend);
    setLeavePos (data_Pos);
  }
  fetchData();
}, []);

  const [dataState, setDataState] = useState({
    telephone: "",
    authPosition: [],
    isManager: "",
    memEmail: "",
    memberID: "",
    originalpasswd: "",
    positionarea: [],
    shift: "",
  });

  const handleRegister = async (e) => {
    e.preventDefault();

    // 基本驗證
    if (!dataState.memberID || !dataState.originalpasswd) {
      toast.error("請填寫所有必填欄位");
      return;
    }

    setIsLoading(true);

    try {
      // 修正：正確的 axios.post 格式
      const response = await axios.post(
        `${config.apiBaseUrl}/schedule/register`,
        // `http://localhost:3009/schedule/register`,
        {
          memberID: dataState.memberID,
          telephone: dataState.telephone,
          memEmail: dataState.memEmail,
          originalpasswd: dataState.originalpasswd,
          positionarea: JSON.stringify(dataState.positionarea),
          shift: dataState.shift,
          authPosition: JSON.stringify(dataState.authPosition),
        },
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      console.log("Registration response:", response.data);

      if (response.data && response.status === 200) {
        setFeedback(true);
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
          const errorMessage = error.response.data?.message || (
            <FormattedMessage
              id="Regis.registerfail"
              defaultMessage="註冊失敗，請檢查輸入資料"
            />
          );
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

  useEffect(() => {
    if (feedback) {
      (async () => {
        try {
          console.log("type of memberID " + typeof dataState.memberID);
          console.log(
            "type of originalpasswd " + typeof dataState.originalpasswd
          );

          const response = await axios.get(
            `${config.apiBaseUrl}/schedule/login`,
            // "http://localhost:3009/schedule/login",
            {
              params: {
                memberid: dataState.memberID,
                password: dataState.originalpasswd,
              },
            }
          );
          console.log("Login response:", response);
          if (response && response.data && response.data.Content[0]) {
            const userData = response.data.Content[0];
            localStorage.setItem("user", JSON.stringify(userData));
            login(userData);
            toast.success("註冊並登入成功！");
          }
        } catch (error) {
          console.error("Login error:", error);
          toast.error("登入過程中發生錯誤");
        }
        setFeedback(false);
      })();

      setDataState({
        authPosition: [],
        memEmail: "",
        memberID: "",
        originalpasswd: "",
        positionarea: [],
        shift: "",
        telephone: "",
      });
      onHide();
    }
  }, [feedback]);

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
            fontSize: "2.7rem",
            fontWeight: "bold",
            color: "white",
            justifyContent: "center",
            width: "100%",
          }}
        >
          <i className="bi bi-person-circle me-2"></i>
          <FormattedMessage id="Regis.sysreg" defaultMessage="系統註冊" />
        </Modal.Title>
      </Modal.Header>

      <Modal.Body className="px-4 py-4">
        {/* 註冊表單 */}
        <Form onSubmit={handleRegister}>
          <Form.Group className="mb-3">
            <Form.Label
              className="fw-bold w-100"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-person me-2"></i>
              <FormattedMessage id="Regis.empID" defaultMessage="工號 *" />
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
              disabled={isLoading}
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
              type="password"
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
              disabled={isLoading}
            >
              <option value="">
                {intl.formatMessage({
                  id: "Regis.req_positionarea",
                  defaultMessage: "請選擇工作區域",
                })}
              </option>
              {Array.isArray(leavePos) && leavePos.map((item) => (
                <option key={item.value} value={item.label}>
                  {item.label}
                  {/* {item.En_lang} */}
                </option>
              ))}
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
              disabled={isLoading}
              required
            >
              <option value="">
                {intl.formatMessage({
                  id: "Regis.req_depart",
                  defaultMessage: "請選擇部門",
                })}
              </option>
              {Array.isArray(leaveGroups) && leaveGroups.map((item) => (
                <option key={item.value} value={item.label}>
                  {item.label}
                  {/* {item.En_lang} */}
                </option>
              ))}
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
              disabled={isLoading}
            >
              <option value="">
                {intl.formatMessage({
                  id: "Regis.req_shift",
                  defaultMessage: "請選擇班別",
                })}
              </option>
              <option value="早班">早班|Morning Shift</option>
              <option value="晚班">晚班|Evening Shift</option>
              <option value="常日班">常日班|Daytime Shift</option>
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
                    註冊中...
                  </>
                ) : (
                  <>
                    <i className="bi bi-person-plus me-2"></i>
                    <FormattedMessage id="Login.reg" defaultMessage="註冊" />
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

export default RegistrPopup;
