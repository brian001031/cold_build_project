import React, { useState } from "react";
import { Modal, Form, Button, Alert, Row, Col } from "react-bootstrap";
import { toast } from "react-toastify";
import { useAuth } from "../../context/GlobalProvider";
import axios from "axios";
import config from "../../config";
import { FormattedMessage, useIntl } from "react-intl";

function LoginPopup({ show, onHide, centered, openModal }) {
  const { login } = useAuth();
  const intl = useIntl();
  const [inputAccount, setInputAccount] = useState("");
  const [inputPassword, setInputPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleRenewLeaveApply = async () => {
    try {
      const response = await axios.get(
        `${config.apiBaseUrl}/absent/compare_leaveApplyDb`,
        // `http://localhost:3009/absent/compare_leaveApplyDb`
      );
      console.log("最新外部/內部請假資料庫對標:", response.data);
    } catch (error) {
      console.error("無法獲取最新外部/內部請假資料庫對標:", error);
    }
  };

  const checkAuth = async(memberID) => {
    try{
      const repsonse = await axios.get(
        `${config.apiBaseUrl}/schedule/checkAuth`,
        // `http://localhost:3009/schedule/checkAuth`,
        {
          params: {
            memberid: String(memberID).trim(),
          },
        }
      )
      console.log("使用者權限資料:", repsonse.data);
      
      return typeof repsonse.data === 'object' && 
      repsonse.data !== null ? repsonse.data : {};
      

    }catch(error){
      console.error("檢查使用者權限失敗:", error);
      return {};
    }

  }
  const handleLogin = async (e) => {
    e.preventDefault(); // 防止表单默认提交
    setError("");

    if (!inputAccount.trim() || !inputPassword.trim()) {
      setError(
        intl.formatMessage({
          id: "Regis.input_emplIDandpwd",
          defaultMessage: "請輸入工號和密碼",
        })
      );
      return;
    }

    setIsLoading(true);
    try {
      const response = await axios.get(
        `${config.apiBaseUrl}/schedule/login`,
        // `http://localhost:3009/schedule/login`,
        {
          params: {
            memberid: String(inputAccount).trim(),
            password: String(inputPassword).trim(),
          },
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      // console.log("API Response status:", response.status);
      // console.log("API Response data:", response.data.Content[0]);
      // console.log("authPosition raw data:", response.data.Content[0]?.authPosition);
      // console.log("positionarea raw data:", response.data.Content[0]?.positionarea);
      

      if (
        response.status === 200 &&
        response.data &&
        response.data.Content &&
        response.data.Content.length > 0
      ) {
        const userData = response.data.Content[0];
        console.log("User data received:", userData);

        const normalizeToArray = (value) => {
          if (!value) return [];
          if (Array.isArray(value)) return value;
          if (typeof value === "string") {
            const trimmed = value.trim();
            if (!trimmed) return [];
            try {
              return JSON.parse(trimmed);
            } catch (parseError) {
              console.warn("auth/area 無法 JSON.parse，改為包裝成陣列:", trimmed, parseError);
              return [trimmed];
            }
          }
          return [];
        };

        let finalAuthPosition = normalizeToArray(userData.authPosition);
        let finalPositionArea = normalizeToArray(userData.positionarea);

        const authStatus = await checkAuth(String(userData.memberID).trim());

        if (authStatus?.data) {
          finalAuthPosition = normalizeToArray(authStatus.data.authPosition);
          finalPositionArea = normalizeToArray(authStatus.data.positionarea);
          
        } else if (Number(userData.authStatus) > 0) {
          console.log("保留登入資料中的主管權限設定。");
        } else {
          console.log("未取得額外權限資訊，沿用登入資料。");
        }

        userData.authPosition = finalAuthPosition;
        userData.positionarea = finalPositionArea;
        userData.authStatus = authStatus?.data?.authStatus ? authStatus?.data?.authStatus : "";

        // console.log("Final user data with auth info:", userData);
        // console.log("authPosition (array):", userData.authPosition);
        // console.log("positionarea (array):", userData.positionarea);

        localStorage.setItem("user", JSON.stringify(userData));


        login(userData);

        setInputAccount("");
        setInputPassword("");
        setError("");

        toast.success("登入成功！");
        onHide();

        // await handleRenewLeaveApply(); // 獲取最新請假資料庫對標
      } else {
        console.warn(
          "Login response indicates no valid user data:",
          response.data
        );
        // setError("帳號或密碼錯誤，請重新輸入");
        // toast.error("帳號或密碼錯誤");
        setError(
          intl.formatMessage({
            id: "Error.failidpwdentry",
            defaultMessage: "帳號或密碼錯誤，請重新輸入",
          })
        );

        toast.error(
          intl.formatMessage({
            id: "Error.failidpwd",
            defaultMessage: "帳號或密碼錯誤",
          })
        );
      }
    } catch (error) {
      console.error("Login error:", error);
      console.error("Error response:", error.response?.data);

      if (error.response?.status === 401) {
        // setError("帳號或密碼錯誤");
        setError(
          intl.formatMessage({
            id: "Error.failidpwd",
            defaultMessage: "帳號或密碼錯誤",
          })
        );
      } else if (error.response?.status === 404) {
        // setError("找不到該用戶");
        setError(
          intl.formatMessage({
            id: "Error.notfindid",
            defaultMessage: "找不到該用戶",
          })
        );
      } else {
        setError(
          intl.formatMessage({
            id: "Error.loginfail_check",
            defaultMessage: "登入失敗，請檢查網路連線或聯繫管理員",
          })
        );
      }
      toast.error(
        intl.formatMessage({
          id: "Error.loginfail",
          defaultMessage: "登入失敗",
        })
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setInputAccount("");
    setInputPassword("");
    setError("");
    setIsLoading(false);
    onHide();
  };

  const handleChangetoRegister = () => {
    setInputAccount("");
    setInputPassword("");
    setError("");
    setIsLoading(false);
    onHide();
    openModal("registerSystem");
  };

  const handleChangetoForgetPassword = () => {
    setInputAccount("");
    setInputPassword("");
    setError("");
    setIsLoading(false);
    onHide();
    openModal("forgetPsw");
  };

  // 批量註冊功能
  const handleRegisterMore = () => {
    // Use a more stable approach for dynamic import to avoid HMR issues
    const loadXLSX = async () => {
      try {
        const XLSX = await import("xlsx");
        return XLSX.default || XLSX;
      } catch (error) {
        console.error("無法載入 XLSX 模組:", error);
        toast.error("無法載入 XLSX 模組，請稍後再試");
        throw error;
      }
    };

    loadXLSX()
      .then((XLSX) => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = ".xlsx,.xls";
        input.onchange = async (e) => {
          const selectedFile = e.target.files[0];
          if (!selectedFile) return;

          console.log(`開始加載 xlsx 檔案: ${selectedFile.name}`);
          toast.info(`開始加載 xlsx 檔案: ${selectedFile.name}`);

          const reader = new FileReader();
          reader.onload = async (evt) => {
            try {
              const data = new Uint8Array(evt.target.result);
              const workbook = XLSX.read(data, { type: "array" });
              const sheetName = workbook.SheetNames[0];
              const worksheet = workbook.Sheets[sheetName];

              console.log("讀取工作表:", sheetName);

              const range = XLSX.utils.decode_range("A2:G500");
              let totalRows = 0;
              let validRows = 0;
              let invalidRows = [];

              for (let row = range.s.r; row <= range.e.r; row++) {
                let rowData = [];

                for (let col = range.s.c; col <= range.e.c; col++) {
                  const cellAddress = XLSX.utils.encode_cell({
                    r: row,
                    c: col,
                  });
                  const cell = worksheet[cellAddress];
                  const cellValue = cell ? cell.v : undefined;
                  rowData.push(cellValue);
                }

                const [
                  memberID,
                  memEmail,
                  telephone,
                  originalpasswd,
                  positionarea,
                  shift,
                  authPosition,
                ] = rowData;

                // 只要A欄（員工工號）有值就處理該行
                if (memberID && String(memberID).trim() !== "") {
                  totalRows++;
                  const excelRowNumber = row + 2; // 因為從 A2 開始，所以需要加 2

                  console.log(`處理第 ${excelRowNumber} 行:`, rowData);

                  // 檢查必要欄位，允許某些欄位為空
                  const processedMemberid = memberID.toString().trim();
                  const processedEmail = memEmail
                    ? memEmail.toString().trim()
                    : "";
                  const processedPassword = originalpasswd
                    ? originalpasswd.toString().trim()
                    : processedMemberid;
                  const processedPositionarea = positionarea
                    ? positionarea.toString().trim()
                    : "";
                  const processedtelephone = telephone
                    ? telephone.toString().trim()
                    : "";
                  const proccessedShift = shift ? shift.toString().trim() : "";
                  const processedAuthPosition = authPosition
                    ? authPosition.toString().trim()
                    : "";

                  // 如果必要欄位都有值才進行註冊
                  if (processedMemberid) {
                    try {
                      const response = await axios.post(
                        `${config.apiBaseUrl}/schedule/register`,
                        // `http://localhost:3009/schedule/register`,
                        {
                          memberID: processedMemberid,
                          memEmail: processedEmail || "",
                          telephone: processedtelephone || "",
                          originalpasswd: processedPassword,
                          positionarea: processedPositionarea || "",
                          shift: proccessedShift || "",
                          authPosition: processedAuthPosition || "",
                        }
                      );

                      validRows++;
                      console.log(
                        `第 ${excelRowNumber} 行註冊成功:`,
                        response.data
                      );
                    } catch (err) {
                      console.error(`第 ${excelRowNumber} 行註冊失敗:`, err);
                      invalidRows.push({
                        row: excelRowNumber,
                        data: rowData,
                        error: err.message,
                      });
                      continue;
                    }
                  }
                  // else {
                  //   console.warn(`第 ${excelRowNumber} 行資料不完整，缺少必要欄位:`, {
                  //         memberid: processedMemberid,
                  //         email: processedEmail || "",
                  //         password: processedPassword ,
                  //         positionarea: processedPositionarea || "",
                  //         telephone: processedtelephone || "",
                  //         shift: proccessedShift || "",
                  //         authPosition: processedAuthPosition || ""
                  //   });
                  //   invalidRows.push({
                  //     row: excelRowNumber,
                  //     data: rowData,
                  //     error: error.message
                  //   });
                  // }
                }
              }

              console.log(
                `批量註冊統計: 總行數=${totalRows}, 成功=${validRows.length}, 失敗=${invalidRows.length}`
              );

              if (validRows.length > 0) {
                toast.success(`批量註冊完成！成功註冊 ${validRows} 筆資料`);
              }

              if (invalidRows.length > 0) {
                toast.warn(
                  `有 ${invalidRows.length} 筆資料處理失敗，請檢查控制台詳情`
                );
                console.log("失敗的行數詳情:", invalidRows);

                // 顯示失敗原因統計
                const errorStats = {};
                invalidRows.forEach((item) => {
                  if (errorStats[item.error]) {
                    errorStats[item.error]++;
                  } else {
                    errorStats[item.error] = 1;
                  }
                });
                console.log("失敗原因統計:", errorStats);
              }

              if (totalRows === 0) {
                toast.info("未找到有效的資料行（A欄員工工號為空）");
              }
            } catch (error) {
              console.error("讀取檔案錯誤:", error);
              toast.error("讀取檔案錯誤，請檢查檔案格式");
            }
          };
          reader.readAsArrayBuffer(selectedFile);
        };
        input.click();
      })
      .catch((error) => {
        // Error is already handled in loadXLSX function
      });
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
          <FormattedMessage id="Login.SysSignin" defaultMessage="系統登入" />
        </Modal.Title>
      </Modal.Header>

      <Modal.Body className="px-4 py-4">
        {error && (
          <Alert variant="danger" className="mb-3 text-center">
            <i className="bi bi-exclamation-triangle me-2"></i>
            {error}
          </Alert>
        )}

        {/* 登入表單 */}
        <Form onSubmit={handleLogin}>
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
                id: "Login.reqiuputid",
                defaultMessage: "請輸入工號",
              })}
              value={inputAccount}
              onChange={(e) => setInputAccount(e.target.value)}
              size="lg"
              disabled={isLoading}
              autoFocus
            />
          </Form.Group>

          <Form.Group className="mb-4">
            <Form.Label
              className="fw-bold"
              style={{ fontSize: "1.2rem", fontWeight: "bold" }}
            >
              <i className="bi bi-lock me-2"></i>
              <FormattedMessage id="Login.pwd" defaultMessage="密碼" />
            </Form.Label>
            <Form.Control
              type="password"
              placeholder={intl.formatMessage({
                id: "Login.reqiuputpwd",
                defaultMessage: "請輸入密碼",
              })}
              value={inputPassword}
              onChange={(e) => setInputPassword(e.target.value)}
              size="lg"
              disabled={isLoading}
            />
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
            {/* 主要操作按鈕 */}
            <Col xs={12} className="mb-2">
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
                    登入中...
                  </>
                ) : (
                  <>
                    <i className="bi bi-box-arrow-in-right me-2"></i>
                    <FormattedMessage id="Login.signin" defaultMessage="登入" />
                  </>
                )}
              </Button>
            </Col>

            {/* 次要操作按鈕 */}
            <Col xs={6}>
              <Button
                variant="outline-secondary"
                size="lg"
                className="w-100"
                disabled={isLoading}
                onClick={() => handleChangetoRegister()}
              >
                <i className="bi bi-person-plus me-1"></i>
                <FormattedMessage id="Login.reg" defaultMessage="註冊" />
              </Button>
            </Col>

            <Col xs={6}>
              <Button
                variant="outline-warning"
                size="lg"
                className="w-100"
                disabled={isLoading}
                onClick={() => handleRegisterMore()}
              >
                <i className="bi bi-file-earmark-excel me-1"></i>
                <FormattedMessage
                  id="Login.xls_reg"
                  defaultMessage="EXCEL註冊"
                />{" "}
              </Button>
            </Col>

            <Col xs={12} className="mt-2">
              <Button
                variant="link"
                className="w-100 text-decoration-none"
                disabled={isLoading}
                onClick={() => handleChangetoForgetPassword()}
              >
                <i className="bi bi-question-circle me-1"></i>
                <FormattedMessage
                  id="Forget.fgtpwd"
                  defaultMessage="忘記密碼"
                />
              </Button>
            </Col>
          </Row>
        </Form>
      </Modal.Body>
    </Modal>
  );
}

export default LoginPopup;
