import React, { useState, useEffect } from "react";
import Button from "react-bootstrap/Button";
//成功提示套件
import { toast } from "react-toastify";


// 按鈕確認组件
const ConfirmSend_Modal = ({ show, onHide, deleteTarget , centered }) => {

//   const { id , mainform_code , name} = deleteTarget;
  const { id, mainform_code, name } = deleteTarget || {};
  const [loading_delete, setLoading_delete] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  if (!show || !deleteTarget) return null; // 不顯示時直接 return null

// 若不仰賴父階層控制則使用以下
//   useEffect(() => {
//     if(deleteTarget !==null)
//         setShowConfirm(true);
//   }, [deleteTarget]);


  const overlayStyle = {
    position: "fixed",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    background: "rgba(0,0,0,0.5)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 999,
};

const modalStyle = {
  background: "white",
  padding: 20,
  borderRadius: 8,
  width: 350,
};


  return (
    <>
     {(
        <div style={overlayStyle}>
            <div style={modalStyle}>
            <h3 style={{ color: "#dc3545" }}>⚠ 刪除確認</h3>
            <p>
                確定要刪除配方單號：<b>{mainform_code}</b>嗎？
                <br />
                操作人: {name}
            </p>

            <div style={{ display: "flex", justifyContent: "flex-end", gap: 10 }}>
                {/* <button onClick={() => setShowConfirm(false)} disabled={loading_delete}> */}
               <button onClick={() => onHide("No")} disabled={loading_delete}>
                取消
                </button>
                <button
                style={{ backgroundColor: "#c21616", color: "white" }}
                disabled={loading_delete}
                onClick={async () => {
                    
                    try {                        
                        setLoading_delete(true);                                            
                        onHide("Yes"); // ✅ 成功後通知父層關閉 Modal , Yes為通知父階層字串             
                        // setShowConfirm(false);
                    } catch (err) {
                        toast.error("刪除失敗: " + err.message);
                    } finally {
                        setLoading_delete(false);                                                 
                    }
                }}
                >
                {loading_delete ? "刪除中..." : "確定刪除"}
                </button>
            </div>
        </div>
      </div>
    )}
  </>
 );
};

export default ConfirmSend_Modal;