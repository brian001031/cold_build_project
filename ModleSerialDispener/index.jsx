import React, { useState, useEffect , Suspense ,useMemo , useCallback } from "react";
import { Button, Table, Form, InputGroup, DropdownButton, Dropdown, Toast } from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import 'moment/locale/zh-tw'; 
import config from '../../config';
import './index.scss';
import axios from "axios";
import { useNavigate , Link } from 'react-router-dom';
import { FormattedMessage, useIntl } from "react-intl";
//成功提示套件
import { toast } from "react-toastify";
import { useAuth } from "../../context/GlobalProvider";
import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import MessagePopup from '../../components/MessagePopup';
import {group_Mes_Final_Dispaner} from "../../mes_remak_data";
import { da } from "zod/v4/locales";
import { json } from "zod";

const filter_title = ["id","allocate_datetime","rule_id"];

//分配機查詢資訊頁面
export default function ModleSerial_Dispener() {  

const navigate = useNavigate();
const { user } = useAuth();
const [finalenable, FinalEnable] = useState(true); // 模擬和正式表單切換
const [option, setOption] = useState('');
const MIN_DATE =  moment("2026-01-01");  // 2026-01-01 預設
const defaultStartDate =  moment().locale("zh-tw").startOf("month");
// 取兩者較早的日期
const finalStartDate = moment.min(defaultStartDate, MIN_DATE);
const [startDate, setStartDate] = useState(finalStartDate);
const [endDay, setEndDay] = useState(moment().locale("zh-tw"));
const [data, setData] = useState([]);
const [totalCount, setTotalCount] = useState(0);    
const [page, setPage] = useState(1);
const [totalPages, setTotalPages] = useState(1);
const [loading, setLoading] = useState(false);
const [mixing_selectmodal, setShowMixing_SelectModal] = useState(false);
const [role_selectmodal, setShowRoleSelectModal] = useState(false);
const [model_selectmodal, setShowmodelModal] = useState(false);
const [selectMixing, setSelectMixing] = useState(null);
const [selectedRole, setSelectedRole] = useState(null);
const [searchTerm, setSearchTerm] = useState("");
const [inputPage, setInputPage] = useState(""); // 新增輸入頁狀態
const [selectedMixing, setSelectedMixing] = useState("");
const [mainquery, setMainQuery] = useState({
    keyword: '',
    info_table: ''
});

// MessagePopup 狀態管理
const [messagePopup, setMessagePopup] = useState({
    show: false,
    type: 'info',
    title: '',
    message: ''
});

// 顯示訊息
const showMessage = useCallback((type, message, title = '') => {
setMessagePopup({
    show: true,
    type,
    title,
    message
});
}, []);


const getSelected_DispanerGroup = (option) => {
  const group = group_Mes_Final_Dispaner.Waiting_For_Real_Dispaner_Name;

  return group[0] ;

//   switch(option){
//     case "":
//       return group[0];
 
//     default:
//       return {};
//   }
};

const prefixRules = [
    {
        option: "模組號Model",
        prefix: "M",
        message: "模組號查詢前綴需要 M 開頭！"
    },
    {
        option: "電芯號Cell",
        prefix: "MW",
        message: "電芯號查詢前綴需要 MW 開頭！"
    }
];
	
const search_direction = finalenable === true        
        ? "total_finalassembly"
        : "test_finalpackage";

const column_list = useMemo (()=>{

 const group = getSelected_DispanerGroup(mainquery.info_table); 
 const availableColumns = Object.keys(group ?? {});
 
  return Object.keys(group).filter(
    key => !filter_title.includes(key)
  );

}, []);

const fetch_Finalassembly_Info = async (page = 1) => {
            setLoading(true);
        try {
            const res = await axios.get(
               `${config.apiBaseUrl}/electricinspec/getfinal_packagedata`,
              // `http://localhost:3009/electricinspec/getfinal_packagedata`,
            {
                params: {            
                  keyword: mainquery.keyword, // 使用輸入的模組號 或 電芯號 (當輸入空則將既有全部數據先行渲染前端)
                  info_table: mainquery.info_table,  // 傳遞選擇的分配機表
                  optionvalue : String(option).trim(),
                  stDate: moment(startDate).format("YYYY-MM-DD"),
                  edDate: moment(endDay).format("YYYY-MM-DD"),
                  sortOrder: 'asc' 
                }
            });


            if(res.status === 200 ){
                // console.log(`得到分配機總ROW資料為 = `+ JSON.stringify(res.data.DispenserRaw,null,2))
                // console.log("總數據數量為 = "+ Object.values(res.data.DispenserRaw).length)
                // console.log("模組號數量為 = "+ String(res.data.allgroup_number) )
                
                //分別取物件結構指向儲存
                const rawData = res.data.DispenserRaw;
                setData(Array.isArray(rawData) ? rawData : []);
                setTotalPages(res.data.allgroup_number);
            }

            
           // setTotalPages(response.data.totalPages);
          //  setPage(response.data.page);
        } catch (err) {
             if (err.response?.status === 404) {
               //  toast.error(err.response.data.message);
                console.log("查詢結果為空：", err.response.data.message);                 
                // showMessage('error',`查詢: ${mainquery.keyword}失敗，請確認搜尋型態再搜尋!`);
                 showMessage(
                    'error',
                    `查詢模組號: ${mainquery.keyword || ''} 失敗，請確認搜尋型態再搜尋!`,
                    '查詢失敗'
                );

            } else {
                console.error("查詢失敗：", err);
               // toast.error("查詢失敗，請稍後再試");
            }
        } finally {
            setLoading(false);
        }
    };

   // 刪除後刷新列表並帶 main_code
  const refreshListToFirstPage = (mainform_code = '') => {
    // 更新 mainquery.keyword
    setMainQuery(prev => ({ ...prev, keyword: mainform_code }));
    setPage(1);              // 跳到第一頁   
  };

  useEffect(() => {
    fetch_Finalassembly_Info(page);
  }, [mainquery]);

// 模組號分組
const modelclass_group = useMemo(() => {

    //確認有數據資料流回傳接收到不為空
    if(data){
        const groups = new Map();

        data.forEach(row => {
            const modelNumber = row?.model_combine_number;

            if (!modelNumber) return;

            if (!groups.has(modelNumber)) {
                groups.set(modelNumber, []);
            }

            groups.get(modelNumber).push(row);
        });

        return Array.from(groups, ([modelNumber, rows]) => ({
            modelNumber,
            rows,
        }));
    }    

}, [data]);



const handleSearch = (e) => {
      e.preventDefault();
	  
	  let prefix_str = "";
      const optionValue = String(option).trim();
      const searchValue = String(searchTerm).trim();

      // 防止選到開始日期或結束日期不符合順序的卡控
      if (moment(startDate) > moment(endDay)) {
          toast.error(`開始日期:${moment(startDate).format("YYYY-MM-DD")}不能比結束日期晚!`);
          return;              
      }else if (moment(endDay) < moment(startDate)) {
          toast.error(`結束日期:${moment(endDay).format("YYYY-MM-DD")}不能比開始日期早!`);
          return;
      }


      //當切換搜尋電池模組號或電芯卡控關鍵條件搜索字樣
	  if( (!searchTerm || searchTerm.length === 0) && option ===""){	  	  
	     toast.error(`請先選擇查詢項目!`);
         return;                 
	  } 
      else {

        const rule = prefixRules.find(item =>
            optionValue.includes(item.option)
        );

        //先確認option 是否已經有選到
        if(!rule){
           toast.error(`請先選擇查詢項目!`);
           return; 
        }
	       
	    if(!isNaN(searchTerm)){		 
		   toast.error(`查詢索引內容不能為全數字!`);
           return;
        }else {

                if (
                    optionValue.includes("模組號Model") &&                                       
                    !/^M\d+-\d+$/i.test(searchValue)             
                ) {
                    toast.error("模組號格式錯誤！必須以 M 開頭，後續只能輸入數字或連字號！");
                    return;
                } 
                //其中 i 代表不區分大小寫，[A-Za-z0-9-]+ 則允許後續包含英文字母、數字與連字號。
                else if (
                    optionValue.includes("電芯號Cell") &&               
                    !/^MW[A-Za-z0-9]+$/i.test(searchValue)                 
                ) {                     
                    toast.error("電芯號格式錯誤！必須以 MW 開頭！");
                    return;
                }

                // const rule = prefixRules.find(item =>
                //     optionValue.includes(item.option)
                // );

                // if (rule && !searchValue.toUpperCase().startsWith(rule.prefix)) {
                //     toast.error(rule.message);
                //     return;
                // }
         }
	  }
	  
     
     
      // 合并 keyword 和 info_table 更新
      setMainQuery({
          ...mainquery, // 保留之前的值
          keyword: searchTerm, // 更新 keyword
          info_table: search_direction // 更新 info_table
      });

      //toast.success(`成功提交搜尋: ${searchValue}`)	  
	  setPage(1);
  };
    
  const currentGroup = modelclass_group[page - 1];
  const pageData = currentGroup?.rows ?? [];
  const prevPage = () => setPage((prev) => Math.max(1, prev - 1));
  const nextPage = () => setPage((prev) => Math.min( totalPages , prev + 1));
  
  
  const goToPage = (value) => {
    
    // 防止超出範圍
    // page > totalPages
    //   ? setPage(totalPages)
    //   : page < 1
    //   ? setPage(1)
    //   : setPage(page);


    const num_page = Number(value);
    if (!num_page || isNaN(num_page)) return;

    if (num_page < 1 || num_page > totalPages) return;

    const safePage = Math.min(Math.max(num_page, 1), totalPages);

    setPage(safePage); // Update the page state
    setInputPage(""); // 清空 input
 
  };


  const convertToExcelRows = (data) => {
    const rows = [];
    let serial = 1;
    let previousModelNumber = null;

    // console.log("確認要轉換EXCEL data:"+ JSON.stringify(data,null,2));
    if (!Array.isArray(data)) {
        console.error("data 不是陣列", data);
        return rows;
    }

    data.forEach((item , idx) => {

        // 取得目前模組號
        const currentModelNumber = item.model_combine_number;

        // 模組號切換時，計數器重設為 1
        if (idx === 0 || currentModelNumber !== previousModelNumber) {
            serial = 1;
        }

          rows.push({
            // "No.":  Number(idx+1) || serial++, 	
            "No.": serial++,		
            "電芯ID": finalenable === true
            ? item.PLCCellID_modinfo ?? ""
            : item.PLCCellID_CE ?? "",
            "K值": item.K_Value ?? "",
            "3.45V充電額定壓降電容量": item.VAHSB ?? "",
            "3.5V~2.8V放電額定壓降電容量": item.VAHSC ?? "",
            "分選號": item.PLCCellIDClass_CE ?? "",
            "盒號": item.PLCTrayID_CE ?? "",
            "後測電壓": item.acirVP12_CE ?? "",
            "內組抗": item.acirRP12_CE ?? "",
            "模組號": item.model_combine_number ?? "未知",
            "排序位置": item.last_define_location ?? "",
			"並聯阻抗值":  item.parallel_match == null || String(item.parallel_match).trim() === "" ? "↑並聯": String(item.parallel_match),
			"分配日期時間":  item.machine_workTime!=="" ?  String(item.machine_workTime) :"未擷取"
          });

          // 記錄目前模組號
        previousModelNumber = currentModelNumber;
      });
      
      return rows;
  };

  const exportExcel =  async(data) => {
    
    if( !Array.isArray(data) || data.length === 0){        
        showMessage(
                'error',
                `請確認搜尋狀態有資料量再轉Excel檔!`,
                '轉換失敗'
        );
       return;
    }


    let rows = [];

    try {
        rows = convertToExcelRows(data);

        // console.log("Step 2：rows =", rows);
        // console.log("Step 3：rows.length =", rows.length);

    } catch (error) {
        console.error("Excel 轉換發生錯誤：", error);
        console.error("錯誤訊息：", error.message);
        console.error("錯誤位置：", error.stack);
    }
        
    const worksheet = XLSX.utils.json_to_sheet(rows);

    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, "分配機電芯模組資訊");

    const excelBuffer = XLSX.write(workbook, {
      bookType: "xlsx",
      type: "array",
    });

    const blob = new Blob([excelBuffer], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });

    console.log("mainquery 後端搜尋表單為:" +mainquery.info_table);

    // const Model_Names = rows["模組號"] ;

    //確認模組號關鍵字過濾重整
    const Model_Names = [ 
        ...new Set(
            rows
                .map(row => row["模組號"])
                .filter(Boolean)
        ),                    
    ];
    
    const modelName_label =
        Model_Names.length === 1
            ? Model_Names[0]
            : "全部模組";

    const customNow = moment().format('YYYY-MM-DD');
    const export_label_str = `${modelName_label}_${customNow}_recordexport.xlsx`;
    saveAs(blob, export_label_str);
  };


return (
  <div className="modledispener_record">

    <div style={{ padding: 20  , width: "120%", margin: "0 auto"}}>
      <h2 style={{ paddingLeft:"70px" , marginTop:"10px", textAlign:"center" , fontStyle:"italic" , fontSize:"2.5em"}}>
	            <FormattedMessage id="scrollingResearching.switchBar.modelTitle" defaultMessage="模組分配機查閱紀錄" />

	     </h2>
		 
	  {loading && <p>Loading...</p>}
	  
	      <div> 
       {/* 查詢區域 */}     
       <InputGroup  className="mb-4 model-search-group"
            >
            <DropdownButton
                variant="outline-secondary"
                title={option || "請選擇"}
                id="input-group-dropdown-1"
            >
                {/* 只有尚未選擇時，才顯示「請選擇」 */}
                {!option && (
                    <Dropdown.Item onClick={() => setOption('')}>
                        請選擇
                    </Dropdown.Item>
                )}
                <Dropdown.Item onClick={() => setOption('模組號Model')}>模組號Model</Dropdown.Item>
                <Dropdown.Item onClick={() => setOption('電芯號Cell')}>電芯號Cell</Dropdown.Item>                
            </DropdownButton>
            <Form.Control
                aria-label="Text input with dropdown button"
                // placeholder= { String(option.trim()).includes("模組號Model")?'請輸入模組號M開頭 例:M09-16398':'請輸入電芯號MW開頭 例:MW2051B10881'}
                placeholder = {
                     option.includes("模組號Model")
                        ? '請輸入模組號M開頭 例:M09-16398'
                        : option.includes("電芯號Cell")
                        ? '請輸入電芯號MW開頭 例:MW2051B10881'
                        : '請先選擇查詢類型'
                }
                value={searchTerm}                
                onChange={e => {
                    const value = e.target.value;
                    setSearchTerm(value);

                    // 當 input 清空時，自動呼叫查詢
                    if (value.trim() === "") {
                       setMainQuery(prev => ({
                            ...prev,
                            keyword: "",
                            info_table: search_direction || "total_finalassembly",
                        }));

                        setPage(1);
                    }
                }}
                onKeyDown={e => {
                    if (e.key === 'Enter') {
                        handleSearch(e);
                    }
                }}
            />
            <Button variant="primary" onClick={handleSearch}>
                搜尋
            </Button>
       </InputGroup>
       	<Form.Group controlId="change_handler" style={{ display: 'flex', alignItems: 'center', marginBottom: '1rem' }}>
                <div style={{ marginRight: '1rem', display: 'flex', flexDirection: 'row', alignItems: 'center'}}> 
                    <div style={{marginRight: "1rem"}}>起始日期:</div>
                    <DatePicker
                        selected={startDate.toDate()}
                        minDate={MIN_DATE.toDate()}       // ✅ 最小日期限制                       
                        onChange={(date) => setStartDate(moment(date))}
                        dateFormat="yyyy/MM/dd"
                        className="form-control" 
                    />
                </div>
                <div style={{ marginRight: '1rem', display: 'flex', flexDirection: 'row', alignItems: 'center'}}> 
                    <div style={{marginRight: "1rem"}}>結束日期:</div>
                    <DatePicker
                        selected={endDay.toDate()}
                        minDate={MIN_DATE.toDate()}       // ✅ 最小日期限制 
                        onChange={(date) => setEndDay(moment(date))}
                        dateFormat="yyyy/MM/dd"
                        className="form-control"
                    />
                </div>
                <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>
                    {/* <Button 
                        className="btn-excel"
                        // variant="btn btn-outline-primary"
                        onClick={() => exportExcel(data)}                        
                    >
                        <i className="bi bi-arrow-left me-1"></i> 轉為Excel
                    </Button> */}

                    {/*非用原生React Bootstrap元件*/}
                    <Button
                        type="button"
                        className="btn-excel"
                        onClick={() => {
                            console.log("Excel 按鈕已觸發");
                            exportExcel(data);
                        }}
                    >
                        <i className="bi bi-file-earmark-excel me-1"></i>
                        轉為 Excel
                    </Button>                
                </div>
            </Form.Group>
      </div>
        
  <div style={{ maxHeight: 700 , overflow: "auto" , border: "1px solid #ddd"}}>
                <Table  
                      striped
                      bordered
                      hover
                      style={{ textAlign: "center", verticalAlign: "middle" , width: "100%", marginBottom: 0,borderCollapse: "collapse"}}       
                    >	  
                    <thead style={{ 
                                      position: "sticky", 
                                      top: 0, 
                                      backgroundColor: "#f8f9fa",
                              }}>
                            <tr>			
                          {/*取物件內指向label {[]}*/}
                        {Object.values(group_Mes_Final_Dispaner).flatMap( group => {
                           const labelMap = group[0]? group[0]:{};                            
                           return Object.entries(labelMap)
                           .filter(([key]) => !filter_title.includes(key))                     
                           .map(([key, label]) => (
                                <th key={key} style={{
                                    minWidth: 120,
                                    backgroundColor:"#FFFF00",
                                    fontWeight: "bold",
                                    // display: "inline-block",
                                 }}>								  
                                    {label || key}						 
                                </th>
                            ));
                        })}
                      </tr>
                  </thead>
                  <tbody>
                    {loading? ( Array.from({ length: 8 }).map((_, rowIdx) => (
                            <tr key={rowIdx}>
                              {column_list.map((col, colIdx) => (
                                <td key={colIdx}>
                                  <div className="skeleton-cell" 
                                       style={{ width: `${60 + Math.random() * 40}%` }}
                                  ></div>
                                </td>
                              ))}
                            </tr>
                        ))
                    ): pageData.length > 0 ? (
                           pageData.map((row, idx) => (
                            <tr key={row.id + '-' + idx}>
                                {column_list.map(col => (
                                    <td key={col} style={{ 
                                         minWidth: "150px", 
                                         maxWidth: "fit-content", 
                                         width: "auto", padding: "8px",
                                        textAlign: "center",
                                        whiteSpace: "nowrap",
                                        overflow: "hidden",
                                        textOverflow: "ellipsis",
                                    }}>
                                        {row[col] !== null ? row[col] : ''}                                        
                                    </td>
                                  ))
                                }
                            </tr>
                        ))
                    ) : (
                            <tr>
                                <td colSpan={column_list.length}>
                                    目前頁面沒有資料
                                </td>
                            </tr>
                    )}                                           
               </tbody>	  
         </Table>
   </div>
   {/*欄位與資料結構名稱bellow▽ */}
   <div style={{ fontSize: "13px", color: "#480c0f" }} >
        目前模組號：
        <span
            style={{
                fontSize: "22px",
                fontWeight: "bold",
                color: "#0e64e5",
                marginRight: "8px",
            }}
        >
         {currentGroup?.modelNumber ?? "無資料"}                 
        </span>  
        <span
            style={{
                fontSize: "19px",
                fontWeight: "bold",
                color: "#4d118e",
                marginRight: "8px",
            }}
        >
          → 共（{pageData.length} 筆電芯數據）  
         </span>   
   </div>
   <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
              }}
    >
            {/* 左側 */}
            <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "10px",
                  }}
            >
                <button onClick={prevPage} disabled={page === 1}>
                  ◀ Prev|上一頁
                </button>
                <span style={{ margin: "0 10px" }}>
                  目前頁數Page {page >totalPages?totalPages:page} / {totalPages}
                </span>
                <button onClick={nextPage} disabled={page === totalPages}>
                  Next|下一頁 ▶
                </button>
                <input
                  type="number"
                  placeholder="頁數"
                  value={inputPage}
                  onChange={(e) => setInputPage(e.target.value)}
                  className="w-8 px-1 py-1 border rounded"
                  onKeyDown={(e) =>{
                     if (e.key !== "Enter") return; 

                     e.preventDefault(); //  防止 form / input 重送
                     goToPage(inputPage);                     
                  }}
                />
                    <button
                      onClick={() => goToPage(inputPage)}
                      className="px-3 py-1 bg-green-350 text-black rounded"
                    >
                      跳頁
                  </button>  
            </div>			
	   </div>
     </div> 
     {/* 訊息彈窗 */}
        <MessagePopup
            show={messagePopup.show}
            type={messagePopup.type}
            title={messagePopup.title}
            message={messagePopup.message}
            onHide={() => {
                setMessagePopup(prev => ({
                    ...prev,
                    show: false
                }));
            }}
        />
    </div>
   );      
}