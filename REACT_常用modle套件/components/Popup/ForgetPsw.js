import React, { useState } from "react";
import { Modal, Form, Button, Alert, Row, Col } from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth, GlobalProvider } from "../../context/GlobalProvider";
import axios from "axios";
import config from "../../config";
import { FormattedMessage, useIntl } from "react-intl";

const ForgetPsw = ({ show, onHide, centered }) => {
  const [isLoading, setIsLoading] = useState(false);
  const { user, login } = useAuth();
  const intl = useIntl();
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [sendVerify, setSendVerify] = useState(false);
  const [newPsw, setNewPsw] = useState("");
  const [confirmPsw, setConfirmPsw] = useState("");

  const handleRegister = async (e) => {
    e.preventDefault();
    onHide();
  };

  const handleSendCode = async () => {
    try {
      const response = await axios.post(
        // "http://localhost:3009/schedule/forgetPsw",
        `${config.apiBaseUrl}/schedule/forgetPsw`,
        {
          memEmail: String(email).trim(),
        }
      );

      if (response.status === 200) {
        toast.success(
          intl.formatMessage({
            id: "Forget.rec_codefrom_email",
            defaultMessage: "驗證碼已發送到您的電子郵件, 請於郵箱中查收！",
          })
        );
      }

      if (response.data.success === true) {
        setSendVerify(true);
      }
    } catch (error) {
      toast.error(
        intl.formatMessage({
          id: "Forget.senderror_fail",
          defaultMessage: "發送驗證碼失敗，請稍後再試！",
        })
      );
      console.error("發送驗證碼失敗:", error);
      return;
    }
  };

  const handleVerifyCode = async () => {
    try {
      if (newPsw !== confirmPsw) {
        toast.error(
          intl.formatMessage({
            id: "Forget.notmatch",
            defaultMessage: "兩次輸入的密碼不一致，請重新輸入！",
          })
        );
        return;
      }

      if (!code.trim() || !newPsw.trim() || !confirmPsw.trim()) {
        toast.error(
          intl.formatMessage({
            id: "Forget.sucessful_login",
            defaultMessage: "密碼修改成功！正在自動登入...",
          })
        );
        return;
      }

      setIsLoading(true);

      const response = await axios.put(
        `${config.apiBaseUrl}/schedule/changePsw`,
        // "http://localhost:3009/schedule/changePsw",
        {
          email: String(email).trim(),
          code: String(code).trim(),
          newPassword: String(newPsw).trim(),
        }
      );

      console.log("Change password response:", response);

      // 檢查響應是否成功且包含用戶資料
      if (response.status === 200 && response.data && response.data.success) {
        login(response.data.rows[0]); // 使用 useAuth 提供的 login 方法登入用戶
        toast.success(
          intl.formatMessage({
            id: "Forget.sucessful_login",
            defaultMessage: "密碼修改成功！正在自動登入...",
          })
        );

        // 如果 API 返回用戶資料，保存到 localStorage
        if (response.data.userData) {
          const userData = response.data.rows[0];
          localStorage.setItem("user", JSON.stringify(userData));
          console.log("User data saved to localStorage:", userData);
          toast.success("登入成功！");
        }

        // 重置表單狀態
        setEmail("");
        setCode("");
        setNewPsw("");
        setConfirmPsw("");
        setSendVerify(false);

        // 關閉 Modal
        onHide();
      } else {
        // API 返回 200 但操作失敗
        const errorMessage =
          response.data?.message || "密碼修改失敗，請重新確認驗證碼或密碼！";
        toast.error(errorMessage);
        console.warn("Password change failed:", response.data);
      }
    } catch (error) {
      console.error("驗證碼錯誤或網路問題:", error);

      if (error.response) {
        // API 返回錯誤狀態碼
        const status = error.response.status;
        const errorData = error.response.data;

        if (status === 400) {
          toast.error(
            intl.formatMessage({
              id: "Forget.verificode_expire",
              defaultMessage: "驗證碼錯誤或已過期，請重新輸入！",
            })
          );
        } else if (status === 404) {
          toast.error(
            intl.formatMessage({
              id: "Forget.notfindemail",
              defaultMessage: "找不到該電子郵件，請確認輸入正確！",
            })
          );
        } else {
          toast.error(errorData?.message || "密碼修改失敗，請稍後再試！");
        }
      } else {
        // 網路錯誤或其他問題
        toast.error("網路連線問題，請檢查網路狀態後重試！");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setIsLoading(false);
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
          <FormattedMessage id="Forget.fgtpwd" defaultMessage="忘記密碼" />
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
              <FormattedMessage
                id="Forget.emailinfo_write"
                defaultMessage="註冊e-mail 填寫"
              />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder={intl.formatMessage({
                id: "Forget.input_mail",
                defaultMessage: "請輸入您的註冊e-mail",
              })}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              size="lg"
              disabled={isLoading}
              autoFocus
            />
            <Button
              variant="link"
              className="p-0"
              onClick={() => handleSendCode()}
            >
              <FormattedMessage
                id="Forget.send_verifycode"
                defaultMessage="送出驗證碼"
              />
            </Button>
          </Form.Group>

          <Form.Group className="mb-4">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-lock me-2"></i>
              <FormattedMessage
                id="Forget.rec_verrifycode"
                defaultMessage="接收到的驗證碼"
              />
            </Form.Label>
            <Form.Control
              type="text"
              placeholder={intl.formatMessage({
                id: "Forget.newverifycode",
                defaultMessage: "請輸入收到的驗證碼",
              })}
              value={code}
              onChange={(e) => setCode(e.target.value)}
              size="lg"
              disabled={isLoading}
              className="mb-2"
            />
            {sendVerify ? (
              <Row className="justify-content-center">
                <Col xs={12} className="text-center">
                  <Form.Control
                    type="password"
                    placeholder={intl.formatMessage({
                      id: "Forget.entry_newpwd",
                      defaultMessage: "請輸入新的密碼",
                    })}
                    value={newPsw}
                    onChange={(e) => setNewPsw(e.target.value)}
                    className="mb-2"
                    size="lg"
                  />
                  <Form.Control
                    type="password"
                    placeholder={intl.formatMessage({
                      id: "Forget.entry_confirmpwd",
                      defaultMessage: "確認輸入密碼",
                    })}
                    value={confirmPsw}
                    onChange={(e) => setConfirmPsw(e.target.value)}
                    className="mb-2"
                    size="lg"
                  />
                  <Button
                    variant="primary"
                    type="submit"
                    size="lg"
                    className="w-100"
                    disabled={isLoading}
                    onClick={handleVerifyCode}
                  >
                    {isLoading ? (
                      <>
                        <span
                          className="spinner-border spinner-border-sm me-2"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        處理中...
                      </>
                    ) : (
                      intl.formatMessage({
                        id: "Forget.submitchange_pwd",
                        defaultMessage: "確認修改密碼",
                      })
                    )}
                  </Button>
                </Col>
              </Row>
            ) : null}
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
        </Form>
      </Modal.Body>
    </Modal>
  );
};

export default ForgetPsw;
