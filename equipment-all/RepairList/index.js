import React, { useState, useEffect } from "react";
import axios from "axios";
import config from "../../config";
import Table from "react-bootstrap/Table";
import dayjs from "dayjs";
import { Link } from "react-router-dom";
import { Button } from "react-bootstrap";
import { FaCheckCircle, FaExclamationCircle, FaTools } from "react-icons/fa";
import * as XLSX from 'xlsx';

const RepairItem = () => {
  const [Inputvalue, setInputvalue] = useState("");
  const [itemnum, setitemnum] = useState(0);
  const [repairItems, setRepairItems] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [fileName, setFileName] = useState('Repair.xlsx');
  const itemsPerPage = 25; // 每頁顯示的項目數量

  useEffect(() => {
    const fetchRepairItems = async () => {
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/repair/repair_list`
        );
        // console.log(response.data);     
       setRepairItems(response.data);

      } catch (error) {
        console.error("取得資料錯誤", error);
      }
    };

    fetchRepairItems();
  }, []);

  useEffect(() => {
    if (Inputvalue === "") {
     // setRepairItems([]);
      setitemnum(0);
      return;
    }
  }, [Inputvalue,itemnum]);

  const MachineSearchtable = async (e) => {
    const machinename = Inputvalue;
    try {
      const response = await axios.get(
        `${config.apiBaseUrl}/repair/machineerrorlist`,
        {
          params: {
            machinename: machinename, // 這邊搜尋機器名稱回饋索引相關內容
          },
        }
      );
     
      //console.log("抓到"+response.data.length+"筆符合資料量");
      //console.log(response.data);
      
      setRepairItems(response.data);
      setitemnum(response.data.length);
      setCurrentPage(1);
      setFileName('Repair-'+Inputvalue+'.xlsx');

      // for (let i = 0; i < repairItems.length; i++) {
      //   const dumpexceldata = repairItems[i];
      //    console.log("Count = " + (parseInt(i+1)));
      //    console.log(dumpexceldata.id);
      //    console.log(dumpexceldata.time);
      //    console.log(dumpexceldata.machine);
      //    console.log(dumpexceldata.question);
      //    console.log(dumpexceldata.handled);
      //    console.log(dumpexceldata.handling_method);
      //    console.log(dumpexceldata.confirmation_method);

      //   //  const id = dumpexceldata.id;
      //   //  const time = dumpexceldata.time;
      //   //  const machine = dumpexceldata.machine;
      //   //  const question = dumpexceldata.question;
      //   //  const status = (dumpexceldata.handled === 0) ? "未修復": (dumpexceldata.handled === 2)? "觀察中" :"已修復";
      //   //  const handling_method = dumpexceldata.handling_method;
      //   //  const confirmation_method = dumpexceldata.confirmation_method;
      // }
      
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  

  const exportToExcel = () => {

    for (let i = 0; i < repairItems.length; i++) {
       (repairItems[i].handled === 0) ? repairItems[i].handled ="未修復": (repairItems[i].handled === 2)? repairItems[i].handled ="觀察中" :repairItems[i].handled ="已修復";
    }

     // 重整數據結構，移除不需要的鍵
     const keysToRemove = ['place','machine_status', 'photo_path', 'reoperation_time','repair_photo', 'created_at',]; // 要移除的鍵

     const transformedData = repairItems.map(item => {
       // 使用解構賦值來移除多組鍵
       const { ...rest } = item;
       keysToRemove.forEach(key => delete rest[key]); // 移除不需要的鍵
       return rest;
     });

    const worksheet =  XLSX.utils.json_to_sheet(transformedData);

    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');

    // 產生下載鏈接
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName); // 使用者自定義檔案名稱
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  // 計算當前頁面要顯示的項目
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentItems = repairItems.slice(indexOfFirstItem, indexOfLastItem);


  // 分頁切換函數
  const paginate = (pageNumber) => setCurrentPage(pageNumber);

  return (
    <div className="repairList">
      <h2>報修紀錄</h2>
       <div>
        {/* <span>姓名:</span>
        <input type="text" /> <Button variant="primary">搜尋</Button> */}
        搜尋:
        <input
          // className="editmachine-input"
          style={{ marginRight: '5px' , width:'270px' , marginTop:'20px'}}
          type="text"
          value={Inputvalue}
          placeholder="輸入編輯號或設備->名稱或編號"
          onChange={(e) => {
            setInputvalue(e.target.value);
          }}
        />
        
        <Button   
        // className="seachmachine-list"
        align="right"     
        variant="primary"
        onClick={MachineSearchtable}
        >
        搜尋
      </Button>
      
      <p>搜尋符合資料為 {itemnum} 筆</p>
      <button className="excle-put" onClick={exportToExcel} disabled={currentItems.length === 0}>
        匯出Excel 
      </button>
      </div>
      <br />
      <Table
        style={{ textAlign: "center", verticalAlign: "middle" }}
        striped
        bordered
        hover
      >
        <thead>
          <tr>
            <th>編輯</th>
            <th>時間</th>
            <th>姓名</th>
            <th>地點</th>
            <th>設備</th>
            <th>狀況</th>
          </tr>
        </thead>
       
        <tbody>
          {currentItems.map((item, index) => (
            <tr key={item.id}>
              <td>
                <Link to={`/edit/${item.id}`}>{item.id}</Link>
              </td>
              <td>{dayjs(item.time).format("MM/DD HH:mm")}</td>
              <td>{item.name}</td>
              <td>{item.place}</td>
              <td>{item.machine}</td>
              <td
                style={{
                  color:
                    item.handled === 0 || item.handled === null
                      ? "red"
                      : "inherit",
                }}
              >
                {item.handled === 0 || item.handled === null ? (
                  <FaTools color="red" /> // 未修復
                ) : item.handled === 2 ? (
                  <FaExclamationCircle color="orange" /> //觀察中
                ) : (
                  // 觀察中
                  <FaCheckCircle color="green" /> // 已處理
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </Table>

      {/* 分頁控制按鈕 */}
      <div>
        <button
          onClick={() => paginate(currentPage - 1)}
          disabled={currentPage === 1}
        >
          Previous
        </button>
        <span> Page {currentPage} </span>
        <button
          onClick={() => paginate(currentPage + 1)}
          disabled={currentItems.length < itemsPerPage}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default RepairItem;
