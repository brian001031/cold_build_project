import React,{ useState, useEffect , useCallback, useMemo, useRef} from 'react';
import { Button, Table, Form, InputGroup, DropdownButton, Dropdown } from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import axios from 'axios';
import 'moment/locale/zh-tw'; 
import config from '../../../config';
import { useNavigate, useLocation } from 'react-router-dom';
import tz from 'moment-timezone';

import MessagePopup from '../../../components/MessagePopup';



const PastReport = () =>{
     const [data , setData] = useState([]);
     const [pageData, setPageData] = useState({});
     const [nowPage, setNowPage] = useState(1);
     const [loading, setLoading] = useState(false);
     
     const isMountedRef = useRef(true);
     const getDefaultValues = () => {
         const now = moment();
         const currentHour = now.hour();
         
         let defaultDate;
         let defaultShift;
         
         // 如果現在是晚班時間 (20:00 到隔天 8:00)
         if (currentHour >= 20 || currentHour < 8) {
             // 晚班時間顯示早班資訊
             defaultShift = '早班';
             
             // 如果過了午夜12點 (0:00-7:59)，顯示昨天早班
             if (currentHour < 12) {
                 defaultDate = moment().subtract(1, 'day').toDate();
             } else {
                 // 未過12點 (20:00-23:59)，顯示當天早班
                 defaultDate = moment().toDate();
             }
         } else {
             // 早班時間 (8:00-19:59) 顯示晚班資訊
             defaultShift = '晚班';
             defaultDate = moment().subtract(1, 'day').toDate(); // 顯示昨天晚班
         }
         
         return { defaultDate, defaultShift };
     };
     
     const { defaultDate, defaultShift } = getDefaultValues();
     
     const [startDate , setStartDate] = useState(defaultDate);
     const [endDate , setEndDate] = useState(moment().tz('Asia/Taipei').toDate());
     const [dayShift , setDayShift] = useState(defaultShift)
     const [changeFile , setChangeFile] = useState('accordingTimeRange')

    // MessagePopup 狀態管理
    const [messagePopup, setMessagePopup] = useState({
        show: false,
        type: 'info',
        title: '',
        message: ''
    });
    
    // 顯示訊息的函數
    const showMessage = useCallback((type, message, title = '') => {
        setMessagePopup({
        show: true,
        type,
        title,
        message
        });
    }, []);
      // 關閉訊息的函數
      const hideMessage = useCallback(() => {
        setMessagePopup(prev => ({ ...prev, show: false }));
      }, []);

    const handleChangePage = useCallback((newPage) => {
        if (newPage === nowPage || loading) return;
        if (newPage < 1 || (pageData.totalPages && newPage > pageData.totalPages)) return;
        
        setNowPage(newPage);
    }, [nowPage, loading, pageData.totalPages]);

     const fetchData = useCallback(async () => {
        if (loading) return; // 防止重複請求
        const now = moment().local('zh-tw').toDate();

        // 決定查詢用的 startDate/endDate 變數，避免直接賦值給 state
        let queryStartDate = startDate;
        let queryEndDate = endDate;
        if (!queryEndDate) { queryEndDate = queryStartDate}
            
        

        setLoading(true);
        try {
            const response = await axios.get(
                `${config.apiBaseUrl}/mixprocess/pastReport`,
                // `http://localhost:3009/mixprocess/pastReport`,
                {
                    params: {
                        startDate: queryStartDate instanceof Date && !isNaN(queryStartDate.getTime()) ? moment(queryStartDate).format('YYYY-MM-DD HH:mm:ss') : "",
                        endDate: queryEndDate instanceof Date && !isNaN(queryEndDate.getTime()) ? moment(queryEndDate).format('YYYY-MM-DD HH:mm:ss') : moment(now).format('YYYY-MM-DD 23:59:59'),
                        dayShift: dayShift || "",
                        page: nowPage,
                        pageSize: 10
                    }
                }
            );

            setData(response.data.data || {});
            setPageData(response.data.pagination || {});

        } catch(error) {
            if (isMountedRef.current) {
                console.error("Error fetching data:", error);
                showMessage('error', '資料載入失敗，請稍後再試', '載入錯誤');
            }
        } finally {
            if (isMountedRef.current) {
                setLoading(false);
            }
        }
     },  [changeFile, startDate, endDate, dayShift]);


    // 當切換 日期班別 或者 時間區間 時觸發 API 調用
    useEffect(() => {
        // console.log('changeFile changed:', changeFile);
        fetchData();

        const intervalId = setInterval(() => {
            fetchData();
        }, 60*1000); // 每60秒更新一次

        return () => clearInterval(intervalId);
    },  [changeFile, startDate, endDate, dayShift]);

    // 組件卸載時清理
    useEffect(() => {
        return () => {
            isMountedRef.current = false;
        };
    }, []);

    

    return (
        <div style={{ backgroundColor: '#17285eff', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)' }}>
            <div style = {{ fontSize: '24px', fontWeight: 'bold', marginBottom: '5px' , display: 'flex', flexDirection: 'row',  alignItems: 'center' }}>
                <div style={{whiteSpace: 'nowrap' , marginRight: '10px' , color: "White"}}>過去戰報</div>
                <Form.Select
                    style={{ width: '200px' }}
                    aria-label="Default select example"
                    value={changeFile}
                    onChange={(e) => setChangeFile(e.target.value)}
                >
                <option value="accordingTimeRange">依時間區間</option>
                </Form.Select>
            </div>
            {
                // 依時間區間
                changeFile === "accordingTimeRange" && (
                    <div>
                        <div 
                        style={{ 
                            display: 'flex',
                            padding: '0',
                            margin: '5px 20px 5px 0',
                        }}
                        // 你可以移除上面的 style，直接使用 d-flex 類別
                        className="d-flex align-items-center"
                    >
                        {/* 第一個群組：選定日期 */}
                        <div className="d-flex align-items-center me-3"> 
                            <div className="fw-bold me-3 text-nowrap" style={{ color: "White"}}>起始時間</div>
                            <DatePicker
                                style={{ height: 'full'}}
                                selected={startDate}
                                onChange={(date) => setStartDate(date)}
                            />
                        </div>
                        <div className="d-flex align-items-center me-3"> 
                            <div className="fw-bold me-3 text-nowrap" style={{ color: "White"}}>結束時間</div>
                            <DatePicker
                                style={{ height: 'full'}}
                                selected={endDate}
                                onChange={(date) => {
                                    // 先檢查限制
                                    if (startDate && moment(date).isBefore(moment(startDate.setHours(0,0,0,0)))) {
                                        showMessage('error', '結束時間需大於起始時間！End time must be greater than start time!', '時間錯誤 Time Error');
                                        return;
                                    }
                                    setEndDate(date);
                                }}
                            />
                        </div>


                        {/* 第二個群組：班別 */}
                        <div className="d-flex align-items-center">
                            <div className="fw-bold me-3 text-nowrap" style={{ color: "White"}}>班別</div>
                            <Form.Select
                                aria-label="選定班別"
                                value={dayShift}
                                onChange={(e) => setDayShift(e.target.value)}
                            >
                                <option value="">請選擇班別</option>
                                <option value="早班">早班</option>
                                <option value="晚班">晚班</option>
                            </Form.Select>
                        </div>
                         {/* 查詢按鈕 */}
                        <Button
                        style={{ marginLeft: '10px' }}
                        onClick={() => {
                            setNowPage(1);
                            fetchData(1);
                        }}
                        
                        >
                            查詢
                        </Button>
                    </div>
                        <Table striped bordered hover></Table>
                        <Table striped bordered hover>
                            <thead>
                                <tr>
                                    <th>站別</th>
                                    <th>混漿批數</th>
                                    <th>N值(最後一筆)</th>
                                    <th>黏度(最後一筆)</th>
                                    <th>粒徑(最後一筆)</th>
                                    <th>固含量(最後一筆)</th>
                                    <th>作業人員</th>
                                </tr>
                            </thead>
                            <tbody>
                                {data.length === 0 ? (
                                    <tr>
                                        <td colSpan={8}>{loading ? '載入中...' : '無資料'}</td>
                                    </tr>
                                ) : (
                                    data.map((item, idx) => (
                                        <tr key = {`${item.tableType}-${idx}`}>
                                            {/* 站別 */}
                                            <td>{item.tableType || '-'}</td>
                                            {/* 混漿批數 */}
                                            <td>{item.LotCount}</td>
                                            {/* N值(最後一筆) */}
                                            <td>{item.Nvalue !== undefined && !isNaN(Number(item.Nvalue)) ? Number(item.Nvalue).toFixed(2) : '-'}</td>
                                            {/* N值(最後一筆) */}
                                            <td>{item.Viscosity !== undefined && !isNaN(Number(item.Viscosity)) ? Number(item.Viscosity).toFixed(2) : '-'}</td>
                                            {/* 黏度(最後一筆) */}
                                            <td>{item.ParticalSize !== undefined && !isNaN(Number(item.ParticalSize)) ? Number(item.ParticalSize).toFixed(2) : '-'}</td>
                                            {/* 粒徑(最後一筆) */}
                                            <td>{item.SolidContent !== undefined && !isNaN(Number(item.SolidContent)) ? Number(item.SolidContent).toFixed(2) : '-'}</td>
                                            {/* 固含量(最後一筆) */}
                                            <td>{item.SolidContent !== undefined && !isNaN(Number(item.Member01_No)) ? `
                                             ${String(item.Member01_Name) + "(" + Number(item.Member01_No) + ")"}
                                            ` : '-'}</td>
                                            
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </Table>
                        
                        {/* 分頁控制 */}
                        {pageData.totalPages >= 1 && (
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '10px' }}>
                                <Button 
                                    variant="primary" 
                                    onClick={() => handleChangePage(nowPage - 1)} 
                                    disabled={pageData.hasPrevPage === false || loading}
                                >
                                    上一頁
                                </Button>
                                <div style={{ color: 'white' , fontFamily: '微軟正黑體' , fontWeight: 'bold' }}>
                                    目前頁數: {nowPage} / {pageData.totalPages} 總筆數: {pageData.totalItems}
                                </div>
                                <Button 
                                    variant="primary" 
                                    onClick={() => handleChangePage(nowPage + 1)} 
                                    disabled={pageData.hasNextPage === false || loading}
                                >
                                    下一頁
                                </Button>
                            </div>
                        )}
                    </div>
                )
            }
            {/* MessagePopup 組件 */}
            <MessagePopup
                show={messagePopup.show}
                type={messagePopup.type}
                title={messagePopup.title}
                message={messagePopup.message}
                onHide={hideMessage}
                autoClose={messagePopup.type === 'success'}
                autoCloseDelay={3000}
            />
        </div>
    )

}

export default PastReport;