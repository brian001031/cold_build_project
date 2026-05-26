import "./index.scss";
import React, { useState, useEffect, useRef, useMemo ,Suspense } from "react";
import axios from "axios";
import Form from "react-bootstrap/Form";
// eslint-disable-next-line no-unused-vars
import config from "../../config";
import * as echarts from "echarts/core";
import { ClipLoader } from "react-spinners";
import Skeleton from "react-loading-skeleton";
import * as XLSX from "xlsx";
import "react-loading-skeleton/dist/skeleton.css";

import index from "../Home";
import moment from "moment";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";

const Purchase_View_status = React.lazy(() => import('../../pages/PurchasingMaterialStatus'));
const TaskboardSearch = React.lazy(() => import("../../pages/TaskboardSearch"));
const PopupAllInfo = React.lazy(() => import("../../pages/MES_EquipmentProInfo_reBuild/PopupAllInfo")); 
const FixPopform = React.lazy(() => import("../../components/FixPopform")); 


const request_menuItems = [
    { title: "首頁",  view:"home" },
    { title: "物料入倉分配管控",   view:"subpage"  , component:Purchase_View_status},
    { title: "生產物料號建構存查更新",   view:"popup" , popupKey_modle: PopupAllInfo },
    // { title: "生產資訊", view:"subpage"  , component:TaskboardSearch},
    // { title: "系統設定", view:"popup" , popupKey_modle: FixPopform }
  ];

  // A~Z
 const material_carditems = Array.from(
    { length: 1 },
    (_, i) => ({
      id: i,
      title: String.fromCharCode(65 + i),
      desc: `當前 ${String.fromCharCode(65 + i)} 前綴物料列`
    })
  );


//測試空組態
const DeviceList =[];

const ManufactConsumableRequest = () => {
  const navigate = useNavigate();
  const Material_Popform = React.lazy(() => import("./Material_Popform")); // 物料前綴項目清單  
  const [prefixmaterials, setSelectPrefixMaterials] = useState("");
  const [isOpenQuality, setIsOpenQuality] = React.useState(false);
  const [currentView, setCurrentView] = useState(null);
  const [openPopup, setOpenPopup] = useState(false);
  const [popup_view_modle, setpopup_View_modle] = useState(null);
  const [pupopt_record, setPupoptRecord] = useState("");
	
   const today = new Date().toLocaleDateString("zh-TW", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    weekday: "long"
  });

  const handleMenuClick = (item) => {

    if (item.view === "home") {
       setCurrentView(null);
    }
    if (item.view === "subpage") {
      setCurrentView(item.component);
    }

    if (item.view === "popup") {    
       OpenPopup_Enable(); 
       setpopup_View_modle(() => item.popupKey_modle);
       setPupoptRecord(item.title);
    }
  };

   // 點擊事件
  const handleCardClick = (item) => {
    const { title ,  desc} = item;
    console.log("按下英文字元為:"+ title);
    setSelectPrefixMaterials(title);
    handleShow();
  };

   const handleShow = () => {
    setIsOpenQuality(true);
  };
  const handleOnHide = () => {
    setIsOpenQuality(false);
  };


  const OpenPopup_Enable = () => {
    setOpenPopup(true);
  };

  const handlePopupOnHide = () => {
    setOpenPopup(false);
  };

  //JSX 小寫開頭 = HTML element
  //JSX 大寫開頭 = React Component 
  const PopupViewModle = popup_view_modle;

  // console.log("要啟動的pupop 標題為:" +  pupopt_record);

  return (
     <div className="manufact_consumable_request">
      <div className="container">
        {/* Header */}
        <header className="header">        
          <h2
            className="title"
            style={{
              textAlign: "center",
              verticalAlign: "middle",
              fontSize: "50px",
            }}
          >
            ERP物料配給管控
          </h2>

          <div className="marquee-wrapper">
            <div className="marquee">
              📅 今日日期：{today}　
            </div>
          </div>

        </header>

        {/* Main Body */}
        <div className="main-content">

          {/* Left Menu */}
          <aside className="sidebar">

            <div className="menu-title">
              功能選單
            </div>

            <ul className="menu-list">
               {request_menuItems.map((item, index) => (
                <li
                  key={index}
                  onClick={() => handleMenuClick(item)}
                  className="menu-item"
                >
                  {item.title}
                </li>
               ))}
            </ul>
          </aside>
          {/* Right Content */}
          <section className="content">
            <h2>物料資訊區塊</h2>  
             {
              (currentView && currentView!==null) ? (
                  <React.Suspense fallback={<div>Loading載入中...</div>}>
                  {React.createElement(currentView)}
                </React.Suspense>
              ):
                (                   
                    <div className="card-grid">
                      {material_carditems.map((item) => (
                        <div
                          key={item.id}
                          className="mini-card"
                          onClick={() => handleCardClick(item)}
                        >
                          <div className="card-title">
                            {item.title}
                          </div>
                          <div className="card-desc">
                            {item.desc}
                          </div>
                        </div>
                      ))}
                  </div>                  
                )             
             }       
            
          </section>
         </div>
          {isOpenQuality === true ? (
          <Suspense fallback={<div>Loading...</div>}>
            <Material_Popform
              show={isOpenQuality}
              onHide={handleOnHide}
              item_prefixkey ={prefixmaterials??'A'}
              centered={true}          
            />
          </Suspense>
        ) : null}

        {openPopup === true && PopupViewModle? (
           <Suspense fallback={<div>Loading...</div>}>
          {
             <>
             {pupopt_record.includes("生產物料號建構存查更新") &&
               <PopupViewModle
                show={openPopup}
                onHide={handlePopupOnHide}
                centered={true}
                mes_side={{ test: "test" }}
              />          
             }
             {pupopt_record.includes("系統設定") &&
                 <FixPopform
                  FormMachineList={DeviceList}
                  side={"Repair"}
                  closeModal={handlePopupOnHide}
                />
             }:null 
             </>             
          }          
        </Suspense>
      ) : null}

      </div>
    </div>
  );
 
}

export default ManufactConsumableRequest;