 
import { BrowserRouter, Routes, Route  } from "react-router-dom";
import { Suspense } from "react";
import React, { useState, useEffect } from "react";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import Table from "react-bootstrap/Table";
import { useNavigate, Form } from "react-router-dom";
import axios from "axios";
import config from "../../config";
import dayjs from "dayjs";
import './index.scss';

const AllocatPopup = React.lazy(() => import("../../pages/PurchasingMaterialStatus/allocatpopup")); //分配料彈出視窗


const PurchasingStatusView = () => {
const [request_materialitems, setMaterialItems] = useState([]);  //採購入庫物料類清單(可領區 和待入庫不可領區)
const [open, setOpen] = useState(false);
const [OpenColumn_div, setOpenColumn_div] = useState(false);
const [currentLink, setCurrentLink] = useState("");
const [openGroup, setOpenGroup] = useState({});
const [purchaseItemDetail, setPurchaseItemDetail] = useState({});  //存取當前物料細節data暫存區
const [open_allocate_popup, setOpenAllocatePopup] = useState(false);  //開啟物料分配彈出視窗控制
const [puchaes_allocatedata, setPuchaes_AllocateData] = useState([]);  // promp 引入popup 分配參考


  const handleOpen = (link) => {
    setCurrentLink(link);
    setOpen(true);
  };


  useEffect(() => {
    const fetch_PurchaseItemData = async () => {
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/purchsaleinvtory/getPurchase_LastnewData`
          // "http://localhost:3009/purchsaleinvtory/getPurchase_LastnewData"
        );

        const res_info = response.data;        
        console.log("擷取fetch_PurchaseItemData 回傳為:" + JSON.stringify(res_info.pickpurchase_info,null,2));
        setMaterialItems(res_info.pickpurchase_info);
        
      } catch (error) {
        console.error("取得資料錯誤", error);
      }
    };

    fetch_PurchaseItemData();
  }, []);

const purchaseOKItems =
  request_materialitems.filter(
    (x) => x.source === "purchase_OK"
  );

const purchaseWaitItems =
  request_materialitems.filter(
    (x) => x.source === "purchase_Wait"
  );


const OpenAllocate_Enable = () => {
  setOpenAllocatePopup(true);
};

const handleAllocateOnHide = () => {
  setOpenAllocatePopup(false);
};

const createCacheKey = (
  order_str,
  purchase_str,
  status
) => {

  return `${order_str}_${purchase_str}_${status}`;
};

const toggleGroup = async (formId,  purch_case ,delivery_status) => {

  const cacheKey = createCacheKey(formId, purch_case , delivery_status);

  const isOpen = openGroup[cacheKey];

  setOpenGroup(prev => ({
    ...prev,
    [cacheKey]: !isOpen
  }));

  // 當擷取到單號在當下未審閱過,需要api 新get fetch render 前端呈現
  if (!purchaseItemDetail[cacheKey]) {    
    try {
      const response = await axios.get(
          `${config.apiBaseUrl}/purchsaleinvtory/Purchase_Index_Detail`
         // "http://localhost:3009/purchsaleinvtory/Purchase_Index_Detail"
           ,
           {
              params: {
                form_order: formId,
                check_status:delivery_status
              }
           }
        );

        const res = response.data;

      console.log("回傳detial 物料 狀態結果info : "+ JSON.stringify(res?.get_info??[""],null,2))        

      setPurchaseItemDetail(prev => ({
        ...prev,
        [cacheKey]: res?.get_info??[""]
      }));
    } catch (err) {
      console.error(err);
    }
  }
};


const handle_allocate_popup = async (  e , order_str, all_record ) =>{

  e.preventDefault(); //  防止 form / input 重送
  
  OpenAllocate_Enable();

  const allocat_value = all_record.quantity;
  const allocat_unit =  all_record.unit;
  const allocat_pkid = all_record.id;
  const allocat_itemcode = all_record.item_code;
  const allocat_spec = all_record.specification;

  const all_allocat_info = [  order_str , allocat_pkid , allocat_spec , allocat_itemcode , allocat_value , allocat_unit ];

  setPuchaes_AllocateData(all_allocat_info);
}

return (
     <div className="purchasing_material_view"> 
        <div style={{ padding: 24}}>
            <h1 style={{ fontSize:"3rem" , paddingLeft:"260px" , fontStyle:"italic" , columnGap:50}}> 物料採購資訊版</h1>
            <br></br>
            <div
                style={{
                  display: "grid",
                  height: 500,
                  gridTemplateColumns: "1fr 1fr",
                  rowGap: 100,
                  columnGap: 30,          
                  width: "100%"              
                }}
            >
               {/*分成兩區塊*/}
               {OpenColumn_div &&                       
                  <div
                        style={{
                            border: "5px solid #110ebb",
                            borderRadius: 12,                             
                            padding: 30,
                            background: "rgb(247, 189, 32)"
                        }}
                    >
                      { 
                      // request_materialitems.length !==0 &&
                      //   request_materialitems.map((item, index) => {
                      //     const isPurchaseOK = item.source === "purchase_OK";			
                      //       return (
                      //               <div
                      //                   key={index}
                      //                   style={{
                      //                     border: "1px solid #110ebb",
                      //                     borderRadius: 12,
                      //                     padding: 20,
                      //                     background: isPurchaseOK
                      //                       ? "#8BC34A"
                      //                       : "#ecca62",
                      //                     color: "#000",
                      //                     boxShadow:
                      //                       "0 2px 8px rgba(0,0,0,0.2)"
                      //                   }}
                      //                 >
                      //                 {/* 標題 */}
                      //                   <h2>
                      //                     {
                      //                       isPurchaseOK
                      //                         ? "已完成收發料"
                      //                         : "待收發料"
                      //                     }  {`共 ${item.total} 筆單`}
                      //                   </h2>
                      //                   {/* source */}
                      //                   <div
                      //                     style={{
                      //                       marginBottom: 12
                      //                     }}
                      //                   >
                      //                     <strong>Source：</strong>
                      //                     {item.source}
                      //                   </div>
                            
                      //                 <div
                      //                     style={{
                      //                       marginBottom: 12
                      //                     }}
                      //                   >
                      //                     <strong>總數：</strong>
                      //                     {item.total}
                      //                   </div>
                      //                 <div>
                      //                     <strong>Form ID：</strong>
                      //                     <ul
                      //                       style={{
                      //                         marginTop: 10
                      //                       }}
                      //                     >
                      //                       {
                      //                         item.form_ids
                      //                           ?.split(",")
                      //                           .map((id, idx) => (

                      //                             <li
                      //                               key={idx}
                      //                               style={{
                      //                                 marginBottom: 6
                      //                               }}
                      //                             >
                      //                               {id}
                      //                             </li>
                      //                           ))
                      //                       }
                      //                     </ul>
                      //                   </div>
                      //               </div>
                      //             );
                      //         })
                            }
                      </div>                                 
                    }
                    {/* 左邊 */}
                    <div
                      style={{
                        border: "5px solid #0c0c0f",
                        borderRadius: 12,
                        padding: 30,
                        background: "#cbddb7",   
                        minWidth: "210px",        
                        borderRadius: "10px",
                        boxShadow: "10px 10px 3px rgba(204, 170, 125, 0.69)"
                      }}
                    >
                      {
                        purchaseOKItems.map((item, index) => (
                          <div key={index}>
                            <h2 
                               style={{
                                fontSize: "28px",
                                marginBottom: 30,
                                color: "#0b044b",
                                borderBottom: "5px solid #555",
                                paddingBottom: 30,
                                width: 290
                              }}
                            >
                              已完成收發料
                              {` 共 ${item.total} 筆單`}
                            </h2>
                            <div>
                              <strong>狀態:</strong>
                              {item.source} {` - 可領料`}
                            </div>
                            {/* <div>
                              <strong>總數：</strong>
                              {item.total}
                            </div> */}
                            <div>
                              <strong>採購單號：</strong>
                              <ul>
                                {
                                  item.form_ids
                                    ?.split(",")
                                    .map((id, idx) => {                                       
                                      const catchkey =  `${id}_${item.source}_1`;
                                      return (
                                         <li key={idx}>
                                          <div onClick={() => toggleGroup(id, item.source ,1)}
                                              style={{
                                                display: "flex",
                                                alignItems: "center",
                                                gap: "3px",                                                                                                
                                                cursor: "pointer"
                                             }}
                                          >
                                            <span                                                
                                                style={{
                                                  marginBottom: 12,
                                                  cursor: "pointer",
                                                  fontSize: "18px",
                                                  color: "#071553",                                                
                                                  fontWeight: "bold",
                                                  backgroundColor:"#0FFE"
                                                }}                                               
                                            >{id}
                                            </span>                                             
                                            <span
                                              style={{background:"#54f06e" ,transition:"1.3s" , transform: "translate(3px, -5px)",}}
                                               onMouseEnter={(e) => {
                                                  e.target.style.backgroundColor = "#eaebd7dc"; // hover 时的背景色变化
                                                }}
                                                onMouseLeave={(e) => {
                                                  e.target.style.backgroundColor = "#54f06e"; // 恢复原来的背景色
                                                }}
                                            >
                                              {
                                                openGroup[catchkey]
                                                  ? "▼"
                                                  : "▲"
                                              }
                                            </span>                                         
                                          </div> 
                                           {
                                                <div
                                                  style={{
                                                      maxHeight:
                                                        openGroup[catchkey]
                                                          ? "1550px"
                                                          : "0px",
                                                      opacity:
                                                        openGroup[catchkey]
                                                          ? 1
                                                          : 0,
                                                      overflow: "hidden",
                                                      transition:
                                                        "max-height 0.8s ease, opacity 0.5s ease",

                                                      marginTop:
                                                        openGroup[catchkey]
                                                          ? 10
                                                          : 0
                                                    }}
                                                >
                                                  {
                                                    purchaseItemDetail[catchkey]?.map((detail, detailIndex) => (
                                                      <div
                                                        key={detailIndex}
                                                        style={{
                                                          marginTop: 3,
                                                          padding: 10,
                                                          background: "#fff",
                                                          borderRadius: 8,
                                                          border: "1px solid #999"
                                                        }}
                                                      >
                                                       {/* Grid 區塊 */}
                                                    <div
                                                      style={{
                                                        display: "grid",                                                       
                                                        gap: "6px",
                                                        fontSize:"16px",
                                                        marginTop: 10,
                                                        background:"rgb(240, 240, 239)"
                                                      }}
                                                    >

                                                        <div>
                                                          品名：
                                                          {detail.product_name}
                                                        </div>
                                                         <div>
                                                          編碼：
                                                          {detail.item_code}
                                                        </div>
                                                        <div>
                                                          規格：
                                                          {detail.specification}
                                                        </div>

                                                         {/* Grid 區塊 */}
                                                        <div
                                                          style={{
                                                            display: "grid",
                                                            gridTemplateColumns: "1fr 1fr 1fr",
                                                            gap: "6px",
                                                            fontSize:"15px",
                                                            marginTop: 10,
                                                            background:"#FF0"
                                                          }}
                                                        >                                                          
                                                          <div>
                                                            數量：
                                                            {detail.quantity}
                                                          </div>                                                        
                                                          <div>
                                                            單位：
                                                            {detail.unit}
                                                          </div>
                                                          <div> 
                                                              供應商號:{detail.vendor_id}
                                                          </div>
                                                       </div>
                                                      
                                                        {/* 新增內部紅色區塊 */}
                                                        <div
                                                          style={{
                                                            background: "#52a522",
                                                            color: "#fff",
                                                            padding: "10px 12px",
                                                            borderRadius: 6,
                                                            marginBottom: 5,                                                             
                                                            marginLeft: "auto", // 推到右側
                                                            marginBottom: 5,
                                                            maxWidth:90,
                                                            transform: "translate(5px, 5px)",
                                                            transitionDuration: "0.5s",                                                            
                                                            fontWeight: "bold",
                                                            cursor: "pointer",
                                                            boxShadow: "0 4px 10px rgba(24, 203, 216, 0.92)",                                             
                                                          }}
                                                          onMouseEnter={(e) => {
                                                            e.currentTarget.style.background =
                                                              "#1e5aca";
                                                            e.currentTarget.style.transform =
                                                              "scale(1.08) translateY(-3px)";
                                                            e.currentTarget.style.boxShadow =
                                                              "0 8px 18px rgba(0,0,0,0.35)";
                                                          }}

                                                          onMouseLeave={(e) => {
                                                            e.currentTarget.style.background =
                                                              "#52a522";
                                                            e.currentTarget.style.transform =
                                                              "scale(1) translateY(0px)";
                                                            e.currentTarget.style.boxShadow =
                                                              "0 4px 10px rgba(0,0,0,0.25)";
                                                          }}
                                                           onClick={(e) => handle_allocate_popup( e, id, detail)}
                                                        >
                                                          執行分配
                                                        </div>
                                                      </div>
                                                      </div>
                                                    ))
                                                  }
                                                </div>
                                            }
                                        </li>                                                                           
                                       );
                                   })
                                }
                              </ul>
                            </div>
                          </div>
                        ))
                      }
                    </div>
                      {/* 右邊 */}
                      <div
                        style={{
                          border: "5px solid #0e0e13",
                          borderRadius: 30,                          
                          padding: 20,
                          background: "#d48d8b",
                          minWidth: "180px",        
                          borderRadius: "10px",
                          boxShadow: "10px 10px 3px rgba(204, 170, 125, 0.69)"
                        }}
                      >
                      {
                        purchaseWaitItems.map((item, index) => (
                          <div key={index}>
                            <h2
                                style={{
                                fontSize: "30px",                                
                                marginBottom: 30,
                                color: "#0b044b",
                                borderBottom: "5px solid #555",
                                paddingBottom: 10,
                                width: 280
                              }}
                            >
                              待處理
                              {` 共 ${item.total} 筆單`}
                            </h2>
                            <div>                                                             
                               <strong>狀態:</strong>
                                {item.source} {` - 尚未交料`}
                            </div>
                            <div>
                                <strong>採購單號：</strong>
                                 <ul>
                                  {
                                    item.form_ids
                                      ?.split(",")
                                      .map((id, idx) => {
                                        const catchkey =  `${id}_${item.source}_0`;
                                        return (
                                          <li key={idx}>
                                            <div onClick={() => toggleGroup(id, item.source , 0)}
                                               style={{
                                                  display: "flex",
                                                  alignItems: "center",
                                                  gap: "3px",                                                                                                
                                                  cursor: "pointer"
                                              }}
                                              >
                                            <span
                                              key={idx}
                                              style={{
                                                  marginBottom: 12,
                                                  fontSize: "18px",
                                                  color: "#071553",                                                
                                                  fontWeight: "bold",
                                                  backgroundColor:"#0FFE"
                                              }}                                               
                                            >{id}</span>
                                            <span
                                              style={{background:"#54f06e" ,transition:"1.3s" , transform: "translate(2px, -7px)",}}
                                              onMouseEnter={(e) => {
                                                    e.target.style.backgroundColor = "#eaebd7dc"; // hover 时的背景色变化
                                                  }}
                                                  onMouseLeave={(e) => {
                                                    e.target.style.backgroundColor = "#54f06e"; // 恢复原来的背景色
                                                  }}
                                            >
                                              {
                                              openGroup[catchkey]
                                                ? "▼"
                                                : "▲"
                                              }
                                            </span>                                         
                                            </div> 
                                            {
                                              <div
                                                style={{
                                                  maxHeight:
                                                  openGroup[catchkey]
                                                    ? "1550px"
                                                    : "0px",
                                                  opacity:
                                                  openGroup[catchkey]
                                                    ? 1
                                                    : 0,
                                                  overflow: "hidden",
                                                  transition:
                                                  "max-height 0.8s ease, opacity 0.5s ease",

                                                  marginTop:
                                                  openGroup[catchkey]
                                                    ? 10
                                                    : 0
                                                }}
                                              >
                                                {
                                                purchaseItemDetail[catchkey]?.map((detail, detailIndex) => (
                                                  <div
                                                    key={detailIndex}
                                                    style={{
                                                      marginTop: 3,
                                                      padding: 5,
                                                      background:"rgb(240, 240, 239)",
                                                      borderRadius: 8,
                                                      border: "1px solid #0e0202"
                                                    }}
                                                  >  
                                                  
                                                      <div>
                                                        品名：{detail.product_name}
                                                      </div>
                                                      <div>
                                                        編碼：{detail.item_code}
                                                        </div>
                                                      <div> 
                                                        規格：{detail.specification}                                                      
                                                      </div>                                                      
                                                        {/* Grid 區塊 */}
                                                        <div
                                                          style={{
                                                            display: "grid",
                                                            gridTemplateColumns: "1fr 1fr 1fr",
                                                            gap: "6px",
                                                            fontSize:"16px",
                                                            marginTop: 10,
                                                            background:"#FF0"
                                                          }}
                                                        >
                                                        <div>
                                                          數量：{detail.quantity}
                                                        </div>
                                                        <div>
                                                          單位：{detail.unit}
                                                        </div>
                                                        <div> 
                                                        供應商號:{detail.vendor_id}
                                                      </div>
                                                       </div>
                                                        {/* 新增內部紅色區塊 */}
                                                        <div
                                                          style={{
                                                            background: "#ff4d4f",
                                                            color: "#fff",
                                                            padding: "10px 12px",
                                                            borderRadius: 6,
                                                            marginBottom: 5,                                                             
                                                            marginLeft: "auto", // 推到右側
                                                            marginBottom: 5,
                                                            maxWidth:80,
                                                            transform: "translate(2px, 5px)",
                                                            transitionDuration: "0.5s",
                                                            // cursor: "pointer",
                                                            fontWeight: "bold"                                                    
                                                          }}
                                                        >
                                                          待處理
                                                        </div>                                                     
                                                  </div>
                                                  ))                                                  
                                                }
                                              </div>				  
                                            }
                                          </li>                                                                           
                                          );
                                        })
                                    }
                                  </ul>
                               </div>
                           </div>
                          ))
                          }
                          
                        </div>
                        
                    </div>

                  </div>
                   {
                      open_allocate_popup &&  puchaes_allocatedata !== null && (
                        <Suspense fallback={<div>Loading...</div>}>                          
                          <AllocatPopup
                            show={open_allocate_popup}
                            onHide={handleAllocateOnHide}
                            centered={true}
                            allocat_data={puchaes_allocatedata}
                          />

                        </Suspense>
                      )
                    }
            </div>
     );
};


export default PurchasingStatusView;