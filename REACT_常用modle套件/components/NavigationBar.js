// NavigationBar

import React from "react";
import { useState, useEffect } from "react";
import {
  Navbar,
  Nav,
  Container,
  Image,
  Dropdown,
  Button,
} from "react-bootstrap";
import { useAuth } from "../context/GlobalProvider";
import { useLanguage, languages } from "../context/LanguageMultilingual"; //語言管理的 hook
import { use } from "react";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import NotFound from "../pages/NotFound";
import LanguageSwitcher from "./LanguageSwitcher";
import { FormattedMessage, useIntl } from "react-intl";

const NavigationBar = ({ openModal }) => {
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);
  const { user, logout, checkAuth } = useAuth();
  const navigate = useNavigate();
  const intl = useIntl();
  const [username, setUsername] = useState("");
  const { lang, setLang } = useLanguage(); // 取得當前語系與設置語系的函數
  const [isLangMenuOpen, setIsLangMenuOpen] = useState(false); // 用於控制語言選單的顯示狀態

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth < 768);
    };

    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  const handleLogin = () => {
    openModal("loginSystem");
  };

  useEffect(() => {
    if (user) {
      setUsername(user.reg_schedulename);
    } else {
      setUsername("Guest");
    }
  }, [user]);

  // 🔥 修正：登出函數
  const handleLogout = async () => {
    try {
      logout();

      // 清空用戶名
      setUsername("Guest");

      // 顯示登出成功訊息
      toast.success("已成功登出！");

      navigate("/", { replace: true }); // 使用 replace 避免用戶按返回鍵

      console.log("Logout completed, redirected to home");
    } catch (error) {
      console.error("Logout error:", error);
      toast.error("登出過程中發生錯誤");
    }
  };

  // const handleLangChange = (languageCode) => {
  //   setLang(languageCode);
  //   setIsLangMenuOpen(false); // 切換語言後關閉語言選單
  // };

  return (
    <Navbar
      className="m-0 p-2"
      bg="primary"
      data-bs-theme="dark"
      expand="md"
      style={{ whiteSpace: "nowrap" }}
    >
      <Container>
        {/* 品牌圖片 */}
        <Navbar.Brand href="/Check">
          <Image src="/Cold_Logo_Black.png" alt="Logo" height="30" />
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="responsive-navbar-nav" />
        <Navbar.Collapse id="responsive-navbar-nav">
          <Nav className="me-auto">
            {/* 廠務報修 (預留) */}
            {/* <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-factory">
                廠務報修
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/bento">便當</Dropdown.Item>
                <Dropdown.Item href="/bentocount">便當統計</Dropdown.Item>
                <Dropdown.Item href="/Check">出勤</Dropdown.Item>
                <Dropdown.Item href="/factoryRepairRequest">新增</Dropdown.Item>
                <Dropdown.Item href="/factoryRepairList">查詢</Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown> */}
            {/* 設備報修 (預留) */}
            {/* <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-equipment">
                設備報修
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/repairRequest">新增</Dropdown.Item>
                <Dropdown.Item href="/repairList">查詢</Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown> */}
            <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-personnel">
                人員作業
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/taskboard">換班提交</Dropdown.Item>
                <Dropdown.Item href="/TaskboardSearch">換班查詢</Dropdown.Item>
                <Dropdown.Item href="/manufact_material_request">ERP物料配給</Dropdown.Item>
                {/* <Dropdown.Item href="/myabsent_search_info">
                  考勤打卡紀錄查詢
                </Dropdown.Item> */}
                <Dropdown.Item href="/allRecordWork">
                  生產紀錄主頁
                </Dropdown.Item>
                <Dropdown.Item href="/ProductResearching">
                  生產作業查詢系統
                </Dropdown.Item>
                 <Dropdown.Item href="/checklist">
                  檢點表
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
            <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-recycle">
                回收作業
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/classrecyclerequest">新增</Dropdown.Item>
                <Dropdown.Item href="/recyclelist">查詢</Dropdown.Item>
                <Dropdown.Item href="/recyclechart">分析圖表單</Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
            <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-mes">
                MES生產資訊
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/mes_equipmentrecord_rebuild">
                  查閱覽
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
            <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-ec">
                進銷存掃碼電化學
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/task_optionmenu">
                  進入選單頁
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
            <Dropdown>
              <Dropdown.Toggle variant="none" id="dropdown-hr">
                人資系統
              </Dropdown.Toggle>
              <Dropdown.Menu>
                <Dropdown.Item href="/schedule">排班系統</Dropdown.Item>
                <Dropdown.Item href="/attendance">考勤系統</Dropdown.Item>
                <Dropdown.Item href="/bulletinboard_info">
                  公告發送頁
                </Dropdown.Item>
                <Dropdown.Item href="/bulletinboard_vewcheck">
                  公告資訊頁
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </Nav>
          <div className="d-flex ms-auto">
            {user ? (
              <Dropdown>
                <Dropdown.Toggle variant="outline-light" id="dropdown-user">
                  {username}
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item onClick={() => openModal("userProfile")}>
                    <FormattedMessage
                      id="Navigation.selfinfo"
                      defaultMessage="個人資料"
                    />
                  </Dropdown.Item>
                  <Dropdown.Item onClick={() => openModal("todayWork")}>
                    <FormattedMessage
                      id="selectTodayWork"
                      defaultMessage="今日工作站別"
                    />
                  </Dropdown.Item>
                  <Dropdown.Item onClick={handleLogout}>
                    <FormattedMessage
                      id="Navigation.LogOut"
                      defaultMessage="登出"
                    />
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            ) : (
              <Button variant="outline-light" onClick={handleLogin}>
                <FormattedMessage id="Navigation.Login" defaultMessage="登入" />
              </Button>
            )}
          </div>
          <LanguageSwitcher
            lang={lang}
            handleLanguageChange={setLang}
            languages={languages}
          />
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default NavigationBar;
