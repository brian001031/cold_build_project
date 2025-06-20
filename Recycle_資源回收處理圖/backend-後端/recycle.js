require("dotenv").config();
const express = require("express");
const router = express.Router();
const db = require(__dirname + "/../modules/db_connect.js");
const db2 = require(__dirname + "/../modules/mysql_connect.js");
const mysql = require("mysql2");
const multer = require("multer");
const axios = require("axios");
const { Sequelize } = require("sequelize");
const fs = require("fs");
const readline = require("readline");
const path = require("path");
const ExcelJS = require("exceljs");
const XLSX = require("xlsx");
const { parseString } = require("fast-csv");

//存取每月選取存量
let current_amont;

//資源回收項目處理狀況
let checkfixrecycle;

let search_number = 0;

let amont_modifyvalue;

const cyclexlsx = "cyclestats.xlsx";
const targetDir = path.join("C:\\recyclelog");
const backup_cyclestatus = path.join(targetDir, cyclexlsx); // 目标文件路径

// 獲取當前日期
let now = new Date();

// 取得當前年份、月份和日期
let nowyear = now.getFullYear();
let nowMonth = (now.getMonth() + 1).toString().padStart(2, "0"); // 月份從0開始，所以要加1
let nowdate = new Date(nowyear, nowMonth, 0)
  .getDate()
  .toString()
  .padStart(2, "0");
let targetSheetName = parseInt(nowyear);
let current_month_amount = nowMonth.toString() + "月儲存量(公斤/月)"; //索引當前工作月份

const recycleitem = process.env.recycle_item;
const recycle_statspath = process.env.recycle_stats;

const test_token = "3EuY6ByrJAcShp93pLE45u0D4iuLEtvYCqhafEoXybs";
const localmachine = "http://localhost:3000/recycleedit";

const querycycleItem = [
  "廢塑膠混合物",
  "廢木材棧板",
  "非有害油泥",
  "金屬廢料混合物(熱處理)",
  "金屬廢料混合物(物理)",
  "底料NMP",
  "E004NMP(回收)",
  "含鋁混和五金廢料(卷料)",
  "含鋁混和五金廢料(邊/片料)",
  "含銅混和五金廢料(卷料)",
  "含銅混和五金廢料(邊/片料)",
  "廢電子零組件",
  "廢塑膠(紙箱含塑膠混和物)",
  "廢塑膠(鋁塑膜)",
  "廢塑膠(PP膜)",
  "廢銅",
  "廢鋁",
  "廢乾電池",
];

const dbcon = mysql.createPool({
  host: "192.168.3.100",
  user: "root",
  password: "Admin0331",
  database: "hr",
  waitForConnections: true,
  connectionLimit: 5,
  queueLimit: 0,
  multipleStatements: true,
});

const mysql_config = {
  host: "192.168.3.100",
  user: "root",
  password: "Admin0331",
  database: "hr",
  multipleStatements: true,
};

dbcon.once("error", (err) => {
  console.error("Database connection error:", err);
});

// 确保只添加一次错误监听器
if (!dbcon.__errorListenerAdded) {
  dbcon.on("error", (err) => {
    console.error("Database connection error:", err);
  });
  dbcon.__errorListenerAdded = true; // 标记监听器已添加

  //確認連線狀況是否正常
  dbcon.getConnection((err, connection) => {
    if (err) {
      console.error("Error getting connection:", err);
      return err;
    }
  });
  dbcon.promise();
}

// setInterval(() => {
//   disconnect_handler(dbcon);
// }, 900000); // 每15分钟执行一次(1000毫秒X900)

setInterval(() => {
  // 獲取當前日期
  now = new Date();
  // 取得當前年份、月份和日期
  nowyear = now.getFullYear();
  nowMonth = (now.getMonth() + 1).toString().padStart(2, "0"); // 月份從0開始，所以要加1
  nowdate = new Date(nowyear, nowMonth, 0)
    .getDate()
    .toString()
    .padStart(2, "0");
  targetSheetName = parseInt(nowyear);
  current_month_amount = nowMonth.toString() + "月儲存量(公斤/月)"; //索引當前工作月份
}, 86400000); // 每一天执行一次(1000毫秒X86400)

function disconnect_handler(conn) {
  conn = mysql.createConnection(mysql_config);

  conn.connect((err) => {
    if (err) {
      console.log("conn connect err ..... 等2秒嘗試重新連接");
      err && setTimeout(disconnect_handler, 2000); // 等待2秒后重连
    }
  });

  conn.on("error", (err) => {
    if (err.code === "PROTOCOL_CONNECTION_LOST") {
      console.log("conn PROTOCOL_CONNECTION_LOST狀況");
      // db error 重新連線
      disconnect_handler();
    } else {
      throw err;
    }
  });
  console.log("conn 連接DB目前正常運行中");
  return conn;
}

// 配置 Multer 用來保存照片(單一照片寫法)
// const storage = multer.diskStorage({
//   destination: (req, file, cb) => {
//     // 將照片保存在 public的 uploads 目錄下
//     cb(null, "public/uploads");
//   },
//   filename: function (req, file, cb) {
//     if (!file.originalname) {
//       // 如果沒有上傳照片，則直接回傳一個預設的檔案名稱
//       cb(null, "no-photo-" + Date.now());
//       return;
//     }
//     // 如果有上傳照片，則使用原始檔案名稱加上時間戳作為檔案名稱
//     const fileName = path.basename(
//       Buffer.from(file.originalname, "latin1").toString("utf-8")
//     );
//     cb(null, Date.now() + "-" + fileName);
//   },
// });
// 配置 Multer 用於保存照片
const storage = multer.diskStorage({
  // 將照片保存在 public 的 uploads 目錄下
  destination: (req, file, cb) => {
    cb(null, "public/uploads");
  },
  // 決定文件名稱
  filename: function (req, file, cb) {
    const timestamp = Date.now();
    file.originalname = Buffer.from(file.originalname, "latin1").toString(
      "utf-8"
    ); // 將文件名轉換為 URL 安全的格式
    // 構建新的文件名稱，包含現在時間後加上連續數字
    const newFileName = `${timestamp}-${file.originalname}`;
    cb(null, newFileName);
  },
});

const upload = multer({ storage: storage });

//新增回收擔資料
// router.post("/recycle_question", upload.single("photo"), async (req, res) => {
//   try {
//     // 先解開body的資料 並解構出來
//     const { name, time, place, machine, machineStatus, question } = req.body;
//     // console.log(name, time, place, machine, machineStatus, question);
//     console.log("hello");
//     // 檢查是否有上傳照片
//     let photo_path = null;
//     if (req.file) {
//       photo_path = path.basename(req.file.path);
//     }
//     // 將報修紀錄寫入資料庫
//     const sql =
//       "INSERT INTO recycles (name, time, place,machine,machine_status, question, photo_path,handled, created_at) VALUES (?,?,?,?,?,?,?,?,CURRENT_TIMESTAMP)";
//     await db2.query(sql, [
//       name,
//       time,
//       place,
//       machine,
//       machineStatus,
//       question,
//       photo_path,
//       0,
//     ]);
//     //開始處理送資料
//     const config = {
//       headers: {
//         "Content-Type": "application/x-www-form-urlencoded",
//         Authorization: `Bearer ${process.env.lineToken}`,
//       },
//     };

//     // 處理訊息內容
//     const message = `
//     報修人:${name}
//     機器:${machine}
//     區域:${place}
//     目前狀態：${machineStatus}
//     異常狀況：${question}
//     `;

//     await axios.post(
//       "https://notify-api.line.me/api/notify",
//       `message=${message}`,
//       config
//     );

//     res.status(201).json({ message: "資料保存成功" });
//   } catch (error) {
//     console.error("發生錯誤", error);
//     res.status(500).json({
//       message: "資料保存錯誤",
//     });
//   }
// });
// 将公式从一个单元格移到另一个指定位置，并调整公式
async function moveFormula(sourceCell, targetRow, targetCol, worksheet) {
  // 获取公式
  const formula = sourceCell.value.formula;

  // 计算公式的新位置
  const newCell = worksheet.getCell(targetRow, targetCol);

  // 解析公式中的单元格引用
  const formulaPattern = /([A-Z]+[0-9]+)/g;
  let adjustedFormula = formula;

  // 替换公式中的单元格引用
  let match;
  while ((match = formulaPattern.exec(formula)) !== null) {
    const originalRef = match[0];
    const col = originalRef.match(/[A-Z]+/)[0];
    const row = originalRef.match(/[0-9]+/)[0];

    // 计算新的单元格引用
    const newCol = String.fromCharCode(
      col.charCodeAt(0) + (targetCol - sourceCell.col)
    );
    const newRef = `${newCol}${parseInt(row) + (targetRow - sourceCell.row)}`;

    // 替换公式中的单元格引用
    adjustedFormula = adjustedFormula.replace(originalRef, newRef);
  }

  // 设置新单元格的公式
  newCell.value = { formula: adjustedFormula };
}

async function confirm_cyclestatsXLS(
  xlsfilePath,
  sheetname,
  itemcode,
  update,
  newamont_tone,
  modify
) {
  //console.log("xlsfilePath = " + xlsfilePath);

  // const sheetnameall = listWorksheetNames(xlsfilePath, sheetname);

  // 使用 XLSX.readFile 方法读取文件
  const workbook = XLSX.readFile(xlsfilePath);

  // 获取所有工作表的名称
  const sheetNames = workbook.SheetNames;

  // 输出工作表名称
  // console.log("工作表名称:", sheetNames);
  // console.log("targetsheet = " + sheetname);

  const targetfind = sheetname; // 替換為你要查找的工作表名稱

  //已經有當前worksheetname (YYYY)
  if (sheetNames.includes(targetfind.toString())) {
    console.log(`${sheetname} ->worksheet已存在`);

    // 选择工作表
    const sheet = workbook.Sheets[targetfind];
    // 将工作表转为 JSON 数据
    const data = XLSX.utils.sheet_to_json(sheet, { header: 1 });

    // 获取工作表的范围
    const range = XLSX.utils.decode_range(sheet["!ref"]);

    const targetmonth_amount =
      modify === 1 ? amont_modifyvalue : current_month_amount;

    // if (modify === 1) {
    //   console.log("modify === 1");
    //   if (itemcode === 0) itemcode = search_number;
    // }

    let cellValue;
    let cellValue2;
    let cellValue3;

    // 遍历数据并查找目标值的位置
    let rowIndex = -1;
    let colIndex = -1;
    let colIndex_itecode = -1;
    let amont_address;
    let recycleitem_save;

    // console.log("targetmonth_amount= " + targetmonth_amount);
    // console.log("itemcode = " + itemcode);
    // console.log("search_number = " + search_number);

    for (let r = 0; r < data.length; r++) {
      const row = data[r];
      for (let c = 0; c < row.length; c++) {
        //先搜尋當前月分的月儲存量(公斤/月)欄位
        if (row[c] === targetmonth_amount) {
          rowIndex = r;
          colIndex = c;
          //這邊是推算要編輯回收項目的column位置
          colIndex_itecode = c + itemcode;

          console.log(
            `${targetmonth_amount} 存量索引位置-> ${rowIndex},${colIndex} 更新索引位置為-> ${rowIndex},${colIndex_itecode} itemcode=${itemcode}`
          );
          //break;

          const cell_address = { c: colIndex_itecode, r: rowIndex }; // 定义单元格地址
          const cell_ref = XLSX.utils.encode_cell(cell_address); // 获取单元格引用
          const cell = sheet[cell_ref];
          //console.log(cell);

          if (!cell || cell === undefined || isNaN(cell.v)) {
            // 如果单元格为空或值为 undefined，设置为浮点数 0.00
            sheet[cell_ref] = {
              v: 0.0,
              t: "n", // 设置单元格类型为数字
            };
            // sheet[cell_ref].v = Math.round(sheet[cell_ref].v * 100) / 100;
            // sheet[cell_ref].t = "n";
            // cellValue2 = String(sheet[cell_ref].v);
            current_amont = String(sheet[cell_ref].v);
            // console.log(
            //   "单元格目前無值或值为undefined,设置为浮点数 = " +
            //     sheet[cell_ref].v
            // );
          } else {
            //這邊判斷是否有更新當前月份儲存值
            const value =
              update == true
                ? parseFloat(newamont_tone).toFixed(3)
                : parseFloat(cell.v);

            // if (update == true) {
            //   console.log("指定月份单元格原先为= " + cell.v);
            //   console.log(
            //     "指定月份单元格位置为=" +
            //       ` row = ${rowIndex} , col = ${colIndex_itecode}`
            //   );
            //   console.log("单元格重新更新, 设置为= " + value);
            // }

            if (!isNaN(value)) {
              sheet[cell_ref].v = Math.round(value * 1000) / 1000;
              sheet[cell_ref].t = "n";
              // cellValue2 = String(sheet[cell_ref].v);
              current_amont = String(sheet[cell_ref].v);
            }
            // console.log(
            //   "单元格有值且可以转换为浮点数 , 设置为= " + sheet[cell_ref].v
            // );
          }

          // 將單元格值轉換為字符串
          // cellValue2 = String(cell.v);
          // console.log(cellValue2);
        }
      }
      if (rowIndex !== -1) break;
    }

    // // 更新工作表的范围
    // sheet["!ref"] = XLSX.utils.encode_range(
    //   XLSX.utils.decode_range(sheet["!ref"])
    // );

    // 將單元格值轉換為字符串
    // cellValue3 = String(cellValue2.v);
    // console.log("cellValue3 = " + cellValue3);

    //console.log("cellValue2 = " + cellValue2);

    //保存修改后的 Excel 文件
    XLSX.writeFile(workbook, xlsfilePath);

    //return cellValue2;
  } //需要新增目前前年格式名稱worksheetname(ex:2025)
  else {
    // console.log(`${sheetname} ->worksheet不存在,需要新增`);
    const newstrsheet = targetSheetName.toString();

    try {
      const workbook = new ExcelJS.Workbook();
      // 讀取 Excel 文件
      await workbook.xlsx.readFile(xlsfilePath);
      // 添加新的工作表
      const newbackupsheet = workbook.addWorksheet(newstrsheet);
      // 獲取指定名稱的工作表
      if (!newbackupsheet) {
        throw new Error(`Sheet "${newstrsheet}" not found.`);
      }

      //（實現多表頭）
      // newbackupsheet.makeColumns
      newbackupsheet.columns = [
        { header: "項目", key: "EditNumber", width: 5 },
        { header: "廢塑膠混合物", key: "D-0299", width: 10 },
        { header: "廢木材棧板", key: "D-0701", width: 10 },
        { header: "非有害油泥", key: "D-0903", width: 10 },
        { header: "金屬廢料混合物(熱處理)", key: "D-1399(熱處理)", width: 20 },
        { header: "金屬廢料混合物(物理)", key: "D-1399(物理)", width: 20 },
        { header: "底料NMP", key: "D-1504(焚化)", width: 10 },
        { header: "E004NMP(回收)", key: "D-1504(物理)", width: 10 },
        { header: "含鋁混和五金廢料(卷料)", key: "D-2527-1", width: 20 },
        { header: "含鋁混和五金廢料(邊/片料)", key: "D-2527-2", width: 20 },
        { header: "含銅混和五金廢料(卷料)", key: "D-2527-3", width: 20 },
        { header: "含銅混和五金廢料(邊/片料)", key: "D-2527-4", width: 20 },
        { header: "廢電子零組件", key: "E-0217", width: 10 },
        { header: "廢塑膠(紙箱含塑膠混和物)", key: "R-0201-1", width: 20 },
        { header: "廢塑膠(鋁塑膜)", key: "R-0201-2", width: 10 },
        { header: "廢塑膠(PP膜)", key: "R-0201-3", width: 10 },
        { header: "廢銅", key: "R-1302", width: 10 },
        { header: "廢鋁", key: "R-1304", width: 10 },
        { header: "廢乾電池", key: "R-2404", width: 10 },
      ];

      const data = [
        ["01月儲存量(公斤/月)"],
        ["02月儲存量(公斤/月)"],
        ["03月儲存量(公斤/月)"],
        ["04月儲存量(公斤/月)"],
        ["05月儲存量(公斤/月)"],
        ["06月儲存量(公斤/月)"],
        ["07月儲存量(公斤/月)"],
        ["08月儲存量(公斤/月)"],
        ["09月儲存量(公斤/月)"],
        ["10月儲存量(公斤/月)"],
        ["11月儲存量(公斤/月)"],
        ["12月儲存量(公斤/月)"],
        ["本年度已儲存量(噸/單位)"],
        ["最大量(噸/月)"],
        // ["本年度已儲存量(噸/單位)", "=ROUND(SUM(B2:B13)/1000,2)"], // '2' 表示小數第2位  ROUND 四捨五入
        // ["最大量(噸/月)", "=ROUNDUP(LARGE(B2:B13,1)/1000,2)"], // '2' 表示小數第2位  ROUNDUP 無條件進入
      ];

      newbackupsheet.addRows(data);

      newbackupsheet.columns.forEach(function (column) {
        column.alignment = { horizontal: "center", vertical: "middle" };
      });

      // 遍历所有行
      // newbackupsheet.eachRow({ includeEmpty: true }, (row, rowNumber) => {
      //   // 遍历每一行的所有单元格
      //   row.eachCell({ includeEmpty: true }, (cell, colNumber) => {
      //     // 输出行和列信息
      //     console.log(
      //       `Row: ${rowNumber}, Column: ${colNumber}, Value: ${cell.value}`
      //     );
      //   });
      // });

      // 获取公式的原始位置
      const formulaCells = [
        { row: 14, col: 2 }, // B14单元格的公式
        { row: 15, col: 2 }, // B15单元格的公式
      ];

      // 定义要复制的列范围
      const targetStartColumn = 3; // 从第3列开始复制公式
      // const numberOfColumns = 11; // 复制到11列 (原先至廢鋁為止)
      const numberOfColumns = 17; // 复制到17列 (新增混和五金廢料+3,廢塑膠+2,廢乾電池+1)

      formulaCells.forEach(({ row, col }) => {
        let sourceindex = newbackupsheet.getCell(row, col);
        let sourcecell = null;

        // 获取源公式 ,後須將插入公式到特定单元格
        if (row === 14) {
          sourcecell = sourceindex.value = {
            formula: "=ROUND(SUM(B2:B13)/1000,3)",
          };
        } else if (row === 15) {
          sourcecell = sourceindex.value = {
            formula: "=ROUNDUP(LARGE(B2:B13,1)/1000,3)",
          };
        }

        // 复制公式到指定的列
        // for (let i = 0; i < numberOfColumns; i++) {
        //   newbackupsheet.getCell(row, targetStartColumn + i).value =
        //     sourcecell;
        // }

        // 将公式从一个单元格移到另一个指定位置，并调整公式
        for (let i = 0; i < numberOfColumns; i++) {
          const targetCol = col + i + 1;
          moveFormula(sourceindex, row, targetCol, newbackupsheet);
        }
      });

      //addsheetColumnRow(worksheet);

      // 確定要追加的行數
      // const lastRow = worksheet.lastRow;
      // const rowIndex = lastRow ? lastRow.number + 1 : 1; // 如果 worksheet 不存在任何行，從第1行開始
      // console.log("rowIndex 從開始下面這行開始 = " + rowIndex);

      // 插入資料並追蹤行索引
      // let currentRowIndex = worksheet.lastRow.number; // 獲取目前最後一行的行索引
      // currentRowIndex += 1; // 增加行索引
      // worksheet.insertRow(currentRowIndex, newrowData);

      //将修改后的工作表保存到指定路径的 Excel 文件中
      await workbook.xlsx.writeFile(xlsfilePath);
    } catch (error) {
      console.error("Error:", error);
    }
  }
}

router.get("/itemnumber", (req, res) => {
  const query = req.query.query || "";
  // console.log("itemname query = " + query);
  let results = "";
  let searchitem = "";

  const filePath = recycleitem;

  fs.readFile(filePath, "utf8", (err, data) => {
    if (err) {
      return res.status(500).send("Error reading file.");
    }

    const lines = data.split("\n");
    // const searchstr = lines.filter((line) => line.includes(query)).join("<-");

    for (let index = 0; index < lines.length; index++) {
      const str = lines[index].split(",");
      if (str[0].toString() === query) {
        searchitem = str[1].toString();

        //最後一筆不執行(因為前面都有\r\n),其他則執行取長度-1
        if (index !== lines.length - 1)
          searchitem = searchitem.substring(0, searchitem.length - 1);

        search_number = index + 1;
        break;
      }
    }

    // const linesfind = lines.split(",");
    // const regex = new RegExp(query, "gi"); // 'g' flag for global search, 'i' flag for case-insensitive search

    // 遍歷每一行，查找指定的字串
    // lines.forEach((line, index) => {
    //   if (line.includes(searchitem)) {
    //     // lineNumbers.push(index + 1); // 行号从1开始
    //     search_number = index + 1;
    //     // console.log("search_number = " + search_number);
    //   }
    // });

    //console.log("search_number 最終= " + search_number);

    //確認recyclestats worksheet 狀況
    confirm_cyclestatsXLS(
      recycle_statspath,
      targetSheetName,
      search_number,
      false,
      0,
      null
    );

    //判斷目前月存量狀態
    if (current_amont === undefined) {
      current_amont = String(0.0);
    }

    // console.log(searchitem);
    // console.log(`${nowyear}-${nowMonth}月目前存量-> ${current_amont}`);

    results = searchitem + "|" + current_amont;
    res.send(results);
  });
});

router.post(
  "/recycle_question",
  upload.array("photos", 10),
  async (req, res) => {
    try {
      //const { name, time, place, machine, machineStatus, question } = req.body;
      const {
        name,
        submittime,
        region,
        itemname, // item
        itemnumber, //  itemeditnum
        maketonne, // currentdayout
        monthtotaltonne, // addmonthtotal
        cycleStatus,
        question,
      } = req.body;

      //這邊確認cycleStatus 狀況,這邊handle會依照處理結果回饋後續該如何處理
      if (cycleStatus.includes("處理完畢")) {
        checkfixrecycle = 1; // 代表已處理完畢
      } else if (cycleStatus.includes("持續處理中")) {
        checkfixrecycle = 2; // 代表已處理一個狀態,後續持續
      } else {
        checkfixrecycle = 0; //代表回收項目目前沒有處理動作
      }

      // console.log("name = " + name);
      //console.log("submittime = " + submittime);
      // console.log("region = " + region);
      //console.log("itemname = " + itemname);
      // console.log("itemnumber = " + itemnumber);
      //console.log("maketonne = " + maketonne);
      //console.log("monthtotaltonne = " + monthtotaltonne);
      // console.log("cycleStatus = " + cycleStatus);
      // console.log("question = " + question);
      // console.log("checkfixrecycle = " + checkfixrecycle);

      let photo_paths = [];
      console.log("!!!!", req.files);
      1;
      if (req.files && req.files.length > 0) {
        // 遍歷上傳的所有圖片，將其保存到資料庫中
        for (let i = 0; i < req.files.length; i++) {
          const file = req.files[i];
          photo_paths.push(file.filename); // 將檔案路徑保存到 photo_paths 陣列中
        }
      }

      //這邊monthtotaltonne 因為有submit不確定性,這邊先從資料庫將綁定該月份submittime 和 itemname 查詢出來SUM(currentdayout)的數量相加maketonne,在執行insert
      const rawDate = submittime.replace(/\//g, "-");
      const dateObj = new Date(rawDate);
      const thisyear = dateObj.getFullYear(); // 2025
      const thismonth = dateObj.getMonth() + 1; // 6（注意：getMonth() 回傳值是 0-11）

      // const sql_month_amount = `SELECT SUM(DISTINCT currentdayout) AS total_amount FROM recyclefix WHERE submittime LIKE '${year}-${month.toString().padStart(2, "0")}%';`;

      const sql_month_amount = `SELECT MONTH(date_only) AS month, SUM(currentdayout) AS month_total FROM (
                                SELECT DISTINCT DATE(submittime) AS date_only, currentdayout FROM recyclefix WHERE YEAR(submittime) = ${thisyear} AND MONTH(submittime) = ${thismonth} AND itemname = '${itemname}') AS distinct_month_data GROUP BY MONTH(date_only) ORDER BY MONTH(date_only)`;

      const [confirm_thisMonth_aomunt] = await db2.query(sql_month_amount);

      let confirm_result_amount = 0;
      let final_addamount = 0;

      // 確認當前月份的累積量
      if (confirm_thisMonth_aomunt.length > 0) {
        confirm_result_amount = confirm_thisMonth_aomunt[0].month_total;
      } else {
        confirm_result_amount = 0; // 如果沒有資料，則設為0
      }
      console.log(
        `確認當前月份(${thisyear}-${thismonth})的累積量:`,
        confirm_result_amount
      );

      final_addamount =
        parseFloat(confirm_result_amount) + parseFloat(maketonne);

      console.log("轉換後 final_addamount = " + final_addamount);

      // 將回收紀錄寫入資料庫
      const sql =
        //"INSERT INTO recyclefix (name, time, place, machine, machine_status, question, photo_path, handled, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP)";
        "INSERT INTO recyclefix (name, submittime, region, itemname, itemeditnum, currentdayout, addmonthtotal, cycleStatus, question, photo_path, handled, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,CURRENT_TIMESTAMP)";

      dbcon.query(sql, [
        name,
        submittime,
        region,
        itemname,
        itemnumber,
        maketonne,
        final_addamount,
        cycleStatus,
        question,
        photo_paths.join(", "), // 將圖片路徑陣列轉換成字串，用逗號分隔
        checkfixrecycle,
      ]);

      //再來更新目前當前年月份全部日期(nowyear , nowMonth , nowdate) 00:00:00 AND 23:59:59所有跟提交之回收項目
      const startdate = nowyear + "-" + nowMonth + "-01 00:00:00";
      const enddate = nowyear + "-" + nowMonth + "-" + nowdate + " 23:59:59";

      const sql2 = `UPDATE recyclefix SET addmonthtotal = ${final_addamount} WHERE (submittime BETWEEN '${startdate}' AND '${enddate}' AND itemname ='${itemname}')`;

      dbcon.query(sql2);

      //console.log("UPDATE recyclefix SET addmonthtotal 執行完畢繼續");

      const sql3 = "SELECT * FROM recyclefix ORDER BY `id` DESC LIMIT 1";

      const [editnum] = await db2.query(sql3);
      const requestid = editnum[0].id + 1; //這邊mysql應用測試無異常,但傳送後會少1目前原因查無先多補1維持正常,因db2為另一模組handle重新query非同步導致

      //再透過查詢indexitem_rownum ,將"新"累積處理量(當前月份)->(monthtotaltonne) update 數據至 cyclestats.xlsx
      fs.readFile(recycleitem, "utf8", (err, data) => {
        if (err) {
          return res.status(500).send("Error reading file.");
        }

        const lines = data.split("\n");

        for (let index = 0; index < lines.length; index++) {
          const str = lines[index].split(",");
          if (str[0].toString() === itemname) {
            search_number = index + 1;
            // console.log(
            //   "再次查詢cyclestatsXLS !!!!! search_number = " + search_number
            // );
            break;
          }
        }
      });

      console.log(nowyear + "年 ");
      confirm_cyclestatsXLS(
        recycle_statspath,
        targetSheetName,
        search_number,
        true,
        final_addamount,
        null
      );

      if (!fs.existsSync(targetDir)) {
        //console.log("backupxlspath路徑有空,需要重建");
        fs.mkdirSync(targetDir, { recursive: true });
      }

      try {
        //複製文件（同步方式）
        fs.copyFileSync(recycle_statspath, backup_cyclestatus);
      } catch (err) {
        console.error("Error copying from Z:/recycle/cyclestats.xlsx:", err);
      }

      // 送linetoken

      //開始處理送資料
      const config_line = {
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Authorization: `Bearer ${process.env.lineToken_taskboard}`,
          // Authorization: `Bearer ${process.env.lineToken}`,
          //Authorization: `Bearer ${test_token}`,
        },
      };

      const config_Discord = {
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Authorization: `Bearer ${process.env.discord_botToken}`,
        },
      };

      const message = `
----------------
回收提交人:${name}
編號: ${requestid}
項目:${itemname}
提交量(公斤):${maketonne}
本月已處理量(公斤):${final_addamount}
區域:${region}
處理狀態：${cycleStatus}
加註：${question}
`;

      const payload = {
        chat_id: `${process.env.telegram_CHAT_ID}`,
        text: message,
      };
      // const telegramurl = `https://api.telegram.org/bot${telegram_BOT_TOKEN}/sendMessage`;

      // const response = await axios.post(
      //   // "https://notify-api.line.me/api/notify",
      //   //`https://discord.com/api/v10/channels/${process.env.discord_channelId}/messages`,
      //   RECYCLE_REQUEST_URL,
      //   // telegramurl,
      //   // payload
      //   // `message=${message}`,
      //   { content: message }
      // );

      for (let k = 1; k < 2; k++) {
        if (k == 0) {
          await axios.post(
            "https://notify-api.line.me/api/notify",
            `message=${message}`,
            config_line
          );

          console.log("資源回收作業已經提交訊息內容委託LINE");
        } else if (k == 1) {
          const RECYCLE_REQUEST_URL = `${process.env.discord_recycle_submit}`;
          await axios.post(
            // "https://notify-api.line.me/api/notify",
            RECYCLE_REQUEST_URL,
            { content: message },
            config_Discord
          );

          console.log("資源回收作業已經提交訊息內容委託DisCord");
        }
      }

      res.status(201).json({ message: "資料保存成功" });
    } catch (error) {
      console.error("發生錯誤", error, error.response);
      res.status(500).json({
        message: "資料保存錯誤",
      });
    }
  }
);

router.get("/getyearamont", async (req, res) => {
  const { year } = req.query;

  //console.log("收到getyearamont year = " + year);

  try {
    // 從資料庫中擷取回收全年各個項目總累積量紀錄
    //const sql = "SELECT * FROM recyclefix ORDER BY id DESC";
    let charttotalamont = "";
    let itemYearamont;

    for (let c = 0; c < querycycleItem.length; c++) {
      // const sql = `SELECT sum(DISTINCT addmonthtotal) FROM recyclefix WHERE year(submittime) = ${year} AND itemname ='${querycycleItem[c]}'`;
      const sql = `SELECT MONTH(date_only) AS month, SUM(currentdayout) AS month_total FROM (
                   SELECT DISTINCT DATE(submittime) AS date_only, currentdayout FROM recyclefix WHERE YEAR(submittime) = ${year} AND itemname = '${querycycleItem[c]}') AS distinct_month_data GROUP BY MONTH(date_only) ORDER BY MONTH(date_only)`;

      const [recycle_monthlyResults] = await db2.query(sql);

      //舊方法取總量
      // const itemYearamont = parseFloat(
      //   recycle_monthlyResults[0]["sum(DISTINCT addmonthtotal)"]
      // );

      // 初始化每個項目的年度總量
      itemYearamont = 0;

      // 新方法取總量
      //當無任何提交量(每月份累加),則制定為0
      if (recycle_monthlyResults.length === 0) {
        itemYearamont = 0;
      } else {
        // 依照月份資料加總（也可以另存每月細項）
        for (let i = 0; i < recycle_monthlyResults.length; i++) {
          const monthTotal =
            parseFloat(recycle_monthlyResults[i].month_total) || 0;
          itemYearamont += monthTotal;
        }
      }

      if (c < querycycleItem.length - 1)
        charttotalamont = charttotalamont + itemYearamont + ",";
      else charttotalamont = charttotalamont + itemYearamont;
    }

    // console.log("charttotalamont = " + charttotalamont);

    res.status(200).send(charttotalamont); // 將回收全年各個項目總累積量紀錄回傳至前端
    // res.status(200).json(charttotalamont);
  } catch (error) {
    console.error("發生錯誤", error);
    res.status(500).json({
      message: "取得資料錯誤",
    });
  }
});

router.get("/getall_dateinfo", async (req, res) => {
  let alldata = [];
  let alldata2 = [];
  let specifu_amount = 0;
  let tempdate = "cycleitem,";
  let tempamont = "";
  let { selectyear, selectmonth } = req.query;
  selectmonth = selectmonth.toString().padStart(2, "0");
  const daysInMonth = new Date(selectyear, selectmonth, 0)
    .getDate()
    .toString()
    .padStart(2, "0");

  const daysArray = Array.from({ length: daysInMonth }, (_, i) => i + 1);

  for (let k = 0; k < daysArray.length; k++) {
    const YMDate_save =
      selectyear +
      "-" +
      selectmonth +
      "-" +
      daysArray[k].toString().padStart(2, "0");

    if (k < daysArray.length - 1) tempdate = tempdate + YMDate_save + ",";
    else tempdate = tempdate + YMDate_save;
  }

  //先存入資源項目和所有日期
  alldata.push(tempdate);

  try {
    // 從資料庫中擷取回收指定->年月全項目整月每天提交紀錄
    for (let c = 0; c < querycycleItem.length; c++) {
      for (let dt = 0; dt < daysArray.length; dt++) {
        const YMDate_str =
          selectyear +
          "-" +
          selectmonth +
          "-" +
          daysArray[dt].toString().padStart(2, "0");

        // const sql = `SELECT date(submittime) AS dated,sum(currentdayout) AS current_total,itemname AS item FROM hr.recyclefix
        //    WHERE date(submittime)  = '${YMDate_str}' AND itemname='${querycycleItem[c]}'`;

        //因有出現當天日期(時間)內有重複相同提交量在一次submit的情況下,所以這邊需要加上distinct
        const sql = `SELECT date(submittime) AS dated, SUM(currentdayout) AS current_total,  itemname AS item
                      FROM (
                        SELECT DISTINCT
                          date(submittime) AS sub_date,
                          submittime,
                          currentdayout,
                          itemname
                        FROM hr.recyclefix
                        WHERE 
                          date(submittime) = '${YMDate_str}'
                          AND itemname = '${querycycleItem[c]}'
                      ) AS distinct_data
                      GROUP BY sub_date, itemname`;

        const [dayamont] = await db2.query(sql);

        //如果沒有查詢到資料,則制定一個空物件
        if (dayamont.length === 0) {
          dayamont.push({
            dated: YMDate_str,
            current_total: 0,
            item: querycycleItem[c],
          });
        }

        //當日期尚未有提交資料量,制定組態value
        // if (dayamont[0].current_total == null) {
        //   dayamont[0].dated = YMDate_str;
        //   dayamont[0].item = querycycleItem[c];
        //   dayamont[0].current_total = 0;
        // }

        const newYMDate_str =
          selectyear +
          "-" +
          selectmonth +
          "-" +
          daysArray[dt].toString().padStart(2, "0");

        //重新制定格式因為日期會浮動跑掉,所以再次重組日期格式
        dayamont[0].dated = newYMDate_str;

        if (dt < daysArray.length - 1) {
          //先將回收項目名稱填入
          if (dt === 0) {
            tempamont = querycycleItem[c] + ",";
          }
          //再填入當日累積處理量
          tempamont = tempamont + dayamont[0].current_total + ",";
        } else tempamont = tempamont + dayamont[0].current_total;

        // console.log(dayamont);

        //這邊計算每個資源回收項目月總量
        specifu_amount += parseFloat(dayamont[0].current_total);
      }

      alldata.push(tempamont);
      alldata2.push(specifu_amount.toString());

      //清為0
      specifu_amount = 0;
    }

    //console.log(alldata);
    // console.log(alldata2);

    // res.status(210).json(alldata); // 將回收指定年月份全項目每天總累積量紀錄回傳至前端

    res
      .status(210)
      .json({ dayeveryamount: alldata, montheveryamount: alldata2 }); // 將回收指定年月份全項目每天總累積量紀錄及定月總量回傳至前端
  } catch (error) {
    console.error("發生錯誤", error);
    res.status(500).json({
      message: "取得資料錯誤",
    });
  }
});

//取出所有回收單項目指定(單年,單月)的處理總量
router.get("/getamount_specify_YM", async (req, res) => {
  let { selectyear, selectmonth } = req.query;
  let char_specify_YM_amont = "";

  try {
    //找每個月分處理量 ex:09月
    // SELECT addmonthtotal FROM recyclefix WHERE year(submittime) = 2024 AND  month(submittime) = 09 AND itemname ='非有害油泥' order by ID limit 1

    for (let c = 0; c < querycycleItem.length; c++) {
      const sql = `SELECT addmonthtotal FROM recyclefix WHERE year(submittime) = ${selectyear} AND  month(submittime) = ${selectmonth} AND itemname ='${querycycleItem[c]}' order by ID limit 1`;

      // console.log("第" + c + "組= " + sql);
      // 從資料庫中擷取回收選擇單年單月紀錄
      const [recycle_specifyamont] = await db2.query(sql);

      const item_specifyamunt = parseFloat(
        recycle_specifyamont[0]["addmonthtotal"]
      );

      if (c < querycycleItem.length - 1)
        char_specify_YM_amont = char_specify_YM_amont + item_specifyamunt + ",";
      else char_specify_YM_amont = char_specify_YM_amont + item_specifyamunt;
    }

    console.log(char_specify_YM_amont);
    res.status(211).json(char_specify_YM_amont); // 將指定(年,月)的處理總量回收紀錄回傳至前端
  } catch (error) {
    console.error("發生錯誤", error);
    res.status(500).json({
      message: "取得資料錯誤",
    });
  }
});

//列出所有回收單資料
router.get("/recyclelist", async (req, res) => {
  try {
    // 從資料庫中擷取回收紀錄
    const sql = "SELECT * FROM recyclefix ORDER BY id DESC";

    const [recycleRecords] = await db2.query(sql);
    //console.log(recycleRecords);
    res.status(200).json(recycleRecords); // 將回收紀錄回傳至前端
  } catch (error) {
    console.error("發生錯誤", error);
    res.status(500).json({
      message: "取得資料錯誤",
    });
  }
});

//列出個別資源回收提交過資料
router.get("/cyclecaselist", async (req, res) => {
  const { cyclename } = req.query;
  //console.log("cyclename = "+ cyclename);

  try {
    // 從資料庫中擷取回收紀錄
    // const sql = "SELECT * FROM recyclefix ORDER BY id DESC";
    // '%E004NMP%'
    const sql = `SELECT * FROM recyclefix WHERE(recyclefix.itemname LIKE '%${cyclename}%' OR recyclefix.itemeditnum LIKE '%${cyclename}%' ) ORDER BY id DESC`;

    const [singlecycleitem] = await db2.query(sql);
    res.status(200).json(singlecycleitem); // 將全部及個別機器異常報修紀錄回傳至前端
  } catch (error) {
    // disconnect_handler(db);
    console.error("發生錯誤", error);
    res.status(500).json({
      message: "取得資料錯誤",
    });
  }
});

//取得特定回收單號
router.get("/recyclelist/:id", async (req, res) => {
  try {
    const { id } = req.params; // 擷取路由的id
    //console.log("擷取路由的id = " + id);
    // 跟資料庫要資料
    const sql = "SELECT * FROM recyclefix WHERE id = ?";

    const [recycleRecord] = await db2.query(sql, [id]);
    // console.log("????", recycleRecord[0]);
    if (!recycleRecord) {
      return res.status(404).json({ message: "未找到對應資料" });
    }
    if (!recycleRecord[0].photo_path) {
      recycleRecord[0].photo_path = []; // 如果沒有照片，設置為空陣列
    } else {
      recycleRecord[0].photo_path = recycleRecord[0].photo_path
        .split(",")
        .map((path) => path.trim());
    }

    if (!recycleRecord[0].recycle_photo) {
      recycleRecord[0].recycle_photo = []; // 如果沒有照片，設置為空陣列
    } else {
      recycleRecord[0].recycle_photo = recycleRecord[0].recycle_photo
        .split(",")
        .map((path) => path.trim());
    }

    res.status(200).json(recycleRecord);
  } catch (error) {
    console.error("發生錯誤", error);
    res.status(500).json({ message: "異常狀況" });
  }
});

//更新部分資料
router.patch(
  "/recyclelist/:id",
  upload.array("photos", 10),
  async (req, res) => {
    try {
      const { id } = req.params;

      const {
        itemname,
        handled,
        cycleStatus,
        recyclefix_person,
        recyclefixmethod,
        currentdayout,
        addmonthtotal,
        modifydayout,
        recyclefix_time,
      } = req.body;

      let photo_paths = [];
      console.log("!!!!", req.files);
      if (req.files && req.files.length > 0) {
        // 遍歷上傳的所有圖片，將其保存到資料庫中
        for (let i = 0; i < req.files.length; i++) {
          const file = req.files[i];
          photo_paths.push(file.filename); // 將檔案路徑保存到 photo_paths 陣列中
        }
      }

      // console.log(
      //   itemname,
      //   handled,
      //   cycleStatus,
      //   recyclefix_person,
      //   recyclefixmethod,
      //   currentdayout,
      //   addmonthtotal,
      //   modifydayout,
      //   recyclefix_time
      // );

      //修正之後總量數據(原先總量扣掉當初提交量再加上新修正之提交量)
      const Divorigin =
        parseFloat(addmonthtotal).toFixed(3) -
        parseFloat(modifydayout).toFixed(3);
      const current = parseFloat(currentdayout);
      const newaddmonthtotal = (Divorigin + current).toFixed(3);

      // console.log(
      //   "修正結果 currentdayout , newaddmonthtotal , modifydayout  = " +
      //     currentdayout,
      //   newaddmonthtotal,
      //   modifydayout
      // );

      // 構建更新資料的 SQL 查詢語句
      let sql =
        "UPDATE recyclefix SET handled = ?, cycleStatus = ?, recyclefix_person = ?, recyclefixmethod = ?, currentdayout = ? , addmonthtotal = ? , modifydayout = ? , recyclefix_time = ? ";

      const sqlParams = [
        handled,
        cycleStatus,
        recyclefix_person,
        recyclefixmethod,
        currentdayout,
        newaddmonthtotal,
        modifydayout,
        recyclefix_time,
      ];

      // 如果有新照片上傳，則包含照片更新部分
      if (photo_paths.length > 0) {
        sql += ", recycle_photo = ?";
        sqlParams.push(photo_paths.join(", "));
      }

      // console.log(
      //   "handled, modifydayout 修正目前為 = " + handled,
      //   modifydayout
      // );
      //判定是否有修改過提交量(modifydayout) 預設為null:沒變過
      if (modifydayout !== null && currentdayout !== modifydayout) {
        sql += " , havemodify = 1";
      }

      sql += " WHERE id = ?";
      sqlParams.push(id);

      await db2.query(sql, sqlParams);

      //搜尋之前建立此編輯號的月份
      const sql2 = "SELECT * FROM recyclefix WHERE id = ?";

      const [recycleRecords] = await db2.query(sql2, [id]);
      const modifyack = recycleRecords[0].havemodify.readUInt8(0);
      const specificDate = new Date(recycleRecords[0].submittime);

      // 提取年
      const specificYear = specificDate.getFullYear();

      // 提取月份
      const specificMonth = (specificDate.getMonth() + 1)
        .toString()
        .padStart(2, "0"); // 0-11 -> 1-12

      const lastdate = new Date(specificYear, specificMonth, 0)
        .getDate()
        .toString()
        .padStart(2, "0");

      amont_modifyvalue = specificMonth.toString() + "月儲存量(公斤/月)";

      // console.log(`修改搜尋年(工作表單)為: ${specificYear}`);
      // console.log(`修改搜尋月(儲存量)為: ${amont_modifyvalue}`);

      //再來更新目前當前年月份全部日期(specificYear , specificMonth , lastdate) 00:00:00 AND 23:59:59所有跟提交之回收項目
      const modify_start = specificYear + "-" + specificMonth + "-01 00:00:00";
      const modify_end =
        specificYear + "-" + specificMonth + "-" + lastdate + " 23:59:59";

      const sql3 = `UPDATE recyclefix SET addmonthtotal = ${newaddmonthtotal} WHERE (submittime BETWEEN '${modify_start}' AND '${modify_end}' AND itemname ='${itemname}')`;

      //因為異步執行,須將excel表單更新後再UPDATE資料庫 -> recyclefix
      //再透過查詢indexitem_rownum ,將"新"累積處理量(當前月份)->(newaddmonthtotal) update 數據至 cyclestats.xlsx
      fs.readFile(recycleitem, "utf8", (err, data) => {
        if (err) {
          return res.status(500).send("Error reading file.");
        }

        const lines = data.split("\n");

        for (let index = 0; index < lines.length; index++) {
          const str = lines[index].split(",");
          if (str[0].toString() === itemname) {
            search_number = index + 1;
            // console.log(
            //   "再次查詢cyclestatsXLS !!!!! search_number = " + search_number
            // );

            console.log(specificYear + "年 ");
            confirm_cyclestatsXLS(
              recycle_statspath,
              specificYear,
              index + 1,
              true,
              newaddmonthtotal,
              modifyack
            );

            if (!fs.existsSync(targetDir)) {
              //console.log("backupxlspath路徑有空,需要重建");
              fs.mkdirSync(targetDir, { recursive: true });
            }

            try {
              //複製文件（同步方式）
              fs.copyFileSync(recycle_statspath, backup_cyclestatus);
              //console.log("備份cyclestats.xlsx 完畢在C:\\recyclelog");
            } catch (err) {
              console.error(
                "Error copying from Z:/recycle/cyclestats.xlsx:",
                err
              );
            }

            break;
          }
        }
      });

      await db2.query(sql3);

      //開始處理送資料
      const config_line = {
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Authorization: `Bearer ${process.env.lineToken_taskboard}`,
          // Authorization: `Bearer ${process.env.lineToken}`,
          // Authorization: `Bearer ${test_token}`,
        },
      };

      const config_Discord = {
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Authorization: `Bearer ${process.env.discord_botToken}`,
        },
      };

      const message = `
----------------
資源回收更改回報
編號: ${id}
項目名稱: ${itemname}
修改量(公斤): ${currentdayout}
本月已處理量(公斤): ${newaddmonthtotal}
更改人員: ${recyclefix_person}
狀態: ${
        handled === "1"
          ? "處理完畢"
          : handled === "2"
          ? "繼續處理"
          : "變更提交量"
      }
處理方法: ${recyclefixmethod}
----------------
連結: ${process.env.web_recycle}/${id}
`;
      /*連結: ${process.env.web_recycle}/${id}*/
      // 連結: ${localmachine}/${id}

      const payload = {
        chat_id: `${process.env.telegram_CHAT_ID}`,
        text: message,
      };
      //const telegramurl = `https://api.telegram.org/bot${telegram_BOT_TOKEN}/sendMessage`;

      // await axios.post(
      //   //"https://notify-api.line.me/api/notify",
      //   //`https://discord.com/api/v10/channels/${process.env.discord_channelId}/messages`,
      //   // RECYCLE_REQUEST_URL,
      //   telegramurl,
      //   payload
      //   //`message=${message}`,
      //   //{ content: message },
      //   //config
      // );

      for (let k = 1; k < 2; k++) {
        if (k == 0) {
          await axios.post(
            "https://notify-api.line.me/api/notify",
            `message=${message}`,
            config_line
          );

          console.log("已經提交訊息內容委託LINE");
        } else if (k == 1) {
          const RECYCLE_REQUEST_URL = `${process.env.discord_recycle_submit}`;
          await axios.post(
            // "https://notify-api.line.me/api/notify",
            RECYCLE_REQUEST_URL,
            { content: message },
            config_Discord
          );
          console.log("ReCycle修改數據內容已經委託DisCord");
        }
      }

      res.status(200).json({ message: "修正更新成功" });
    } catch (error) {
      console.error("更新錯誤:", error);
      res.status(500).json({ message: "更新錯誤" });
    }
  }
);

module.exports = router;
