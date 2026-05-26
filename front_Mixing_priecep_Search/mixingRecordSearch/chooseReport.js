
import { useState, useEffect } from 'react';
import { Button, Table, Form, InputGroup, DropdownButton, Dropdown } from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import axios from 'axios';
import 'moment/locale/zh-tw'; 
import config from '../../../config';
import { useNavigate } from 'react-router-dom';
import { totalChange } from "../../../mes_remak_data"



const ChooseReport = () => {
    const navigate = useNavigate();
    const [searchTerm, setSearchTerm] = useState("");
    const [option, setOption] = useState('');
    const [startDate, setStartDate] = useState(moment().locale("zh-tw"));
    const [endDay, setEndDay] = useState(moment().locale("zh-tw"));
    const [data, setData] = useState([]);
    const [totalCount, setTotalCount] = useState(0);

    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalPage, setTotalPage] = useState(1);


    //用於轉換欄位名稱
    const info = {    
        "System_Step": "System_Step",
        "EngineerNo": "EngineerNo",
        "EngineerName": "EngineerName",
        "LotNo": "LotNo",
        "Member01_Name": "Member01_Name",
        "Member01_No": "Member01_No",
        "Date": "Date",
        "BatchStart": "BatchStart",
        "BatchEnd": "BatchEnd",
        "batch_time_diff": "Total Batch Time",
        "TransportStart": "TransportStart",
        "TransportEnd": "TransportEnd",
        "Nvalue": "Nvalue",
        "Viscosity": "Viscosity",
        "ParticalSize": "ParticalSize",
        "SolidContent": "SolidContent",
        "LFP_1": "LFP_1",
        "LFP_2": "LFP_2",
        "SuperP_1": "SuperP_1",
        "SuperP_2": "SuperP_2",
        "PVDF_1": "PVDF_1",
        "PVDF_2": "PVDF_2",
        "CNT_1": "CNT_1",
        "CNT_2": "CNT_2",
        "CNT_3": "CNT_3",
        "CNT_4": "CNT_4",
        "NMP_1": "NMP_1",
        "NMP_2": "NMP_2",
        "Graphite1_1": "Graphite1_1",
        "Graphite1_2": "Graphite1_2",
        "Super_P_1": "Super_P_1",
        "Super_P_2": "Super_P_2",
        "CMC_1": "CMC_1",
        "CMC_2": "CMC_2",
        "Graphite_2_1": "Graphite_2_1",
        "Graphite_2_2": "Graphite_2_2",
        "SBR_1": "SBR_1",
        "SBR_2": "SBR_2",
        "NMP_1_1": "NMP_1_1",
        "NMP_1_2": "NMP_1_2",
        "PAA_1": "PAA_1",
        "PAA_2": "PAA_2",
        }
    const handleSearch = (e) => {
        e.preventDefault();
        fetchDate(1); // 搜尋時回到第1頁
    };

    // fetchDate 支援傳入目標頁碼
    const fetchDate = async (targetPage = page) => {
        try {
            const response = await axios.get(
                `${config.apiBaseUrl}/mixprocess/getSearchPage`,
                // "http://localhost:3009/mixprocess/getSearchPage",
                {
                    params: {
                        option: String(option).trim() || '全部資料',
                        searchTerm: String(searchTerm).trim() || '',
                        startDate: startDate.format('YYYY/MM/DD'),
                        endDay: endDay.format('YYYY/MM/DD'),
                        page: targetPage,
                        pageSize: pageSize,
                    },
                    headers: {
                        'Content-Type': 'application/json',
                    },
                }
            );
            
            const responseData = response?.data || {};
            const totalPages = responseData.totalPages || 1;
            const dataArray = responseData.data || [];
            
            // 確保目標頁碼在有效範圍內
            const validTargetPage = Math.max(1, Math.min(totalPages, targetPage));
            
            setData(dataArray);
            setPage(validTargetPage);
            setTotalPage(totalPages);
            setTotalCount(responseData.totalCount || 0);
            
            console.log('Response data:', responseData);
            console.log(`設定頁碼: ${validTargetPage}/${totalPages}, 資料筆數: ${dataArray.length}`);
        } catch (error) {
            console.error('Error fetching data:', error);
            setData([]);
            setTotalPage(1);
            setTotalCount(0);
        }
    };
    useEffect(() => {
        const interval = setInterval(() => {
            fetchDate(page);
        }, 20000);
        return () => clearInterval(interval);
    }, [option, searchTerm, startDate, endDay, page, pageSize]);
    
    const columns = data.length > 0 ? Object.keys(data[0]) : [];

    const handleDownload = async () => {

        try {
            let fileName = 'mixing_record_data';
            if (option === "全部資料") {
                fileName = 'mixing_record_data_all';
            } else if (option === "正極混漿") {
                fileName = 'mixing_record_data_cathode';
            } else if (option === "負極混漿") {
                fileName = 'mixing_record_data_anode';
            } else if (option === "已刪除資訊"){
                fileName = "已刪除資訊"
            }

            const response = await axios.get(
                `${config.apiBaseUrl}/mixprocess/downloadData`,
                // "http://localhost:3009/mixprocess/downloadData",
                {
                    params: {
                        option: String(option).trim() || '全部資料',
                        searchTerm: String(searchTerm).trim() || '',
                        startDate: startDate.format('YYYY/MM/DD'),
                        endDay: endDay.format('YYYY/MM/DD')
                    },
                    responseType: 'blob', 
                }
            );

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `${fileName}-${startDate.format('YYYYMMDD')}-${endDay.format('YYYYMMDD')}.xlsx`); 
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
        } catch (error) {
            console.error('Error downloading data:', error);
        }
    };

    return (
        <div style={{ padding: 24 }}>
            <InputGroup className="mb-3">
                <DropdownButton
                    variant="outline-secondary"
                    title={option || "全部資料"}
                    id="input-group-dropdown-1"
                >
                    <Dropdown.Item onClick={() => setOption('全部資料')}>全部資料</Dropdown.Item>
                    <Dropdown.Item onClick={() => setOption('正極混漿')}>正極混漿</Dropdown.Item>
                    <Dropdown.Item onClick={() => setOption('負極混漿')}>負極混漿</Dropdown.Item>
                    <Dropdown.Item onClick={() => setOption('已刪除資訊')}>已刪除資訊</Dropdown.Item>
                </DropdownButton>
                <Form.Control
                    aria-label="Text input with dropdown button"
                    placeholder='請輸入欲查詢之工程師工號或Lot編號'
                    value={searchTerm}
                    onChange={e => setSearchTerm(e.target.value)}
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
                        onChange={(date) => setStartDate(moment(date))}
                        dateFormat="yyyy/MM/dd"
                        className="form-control" 
                    />
                </div>
                <div style={{ marginRight: '1rem', display: 'flex', flexDirection: 'row', alignItems: 'center'}}> 
                    <div style={{marginRight: "1rem"}}>結束日期:</div>
                    <DatePicker
                        selected={endDay.toDate()}
                        onChange={(date) => setEndDay(moment(date))}
                        dateFormat="yyyy/MM/dd"
                        className="form-control"
                    />
                </div>
                <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}>
                    <Button 
                        variant="btn btn-outline-primary"
                        onClick={handleDownload}
                    >
                        <i className="bi bi-arrow-left me-1"></i> 轉為Excel
                    </Button>
                    <Button 
                        variant="outline-secondary"
                        style={{ marginLeft:'10px' }}
                        onClick={() => {
                            navigate('/productResearching');
                        }}
                    >
                        <i className="bi bi-arrow-left me-1"></i> 選擇其他查詢表
                    </Button>
                </div>
                
            
            </Form.Group>
            <div style={{ maxHeight: 500 , overflow: "auto" , border: "1px solid #ddd"}}>
                <Table striped bordered hover style={{ 
                    width: "100%",
                    marginBottom: 0,
                    borderCollapse: "collapse",
                }}>
                    <thead style={{ 
                        position: "sticky", 
                        top: 0, 
                        backgroundColor: "#f8f9fa",
                    }}>
                        <tr>
                            {columns.map((col, index) => (
                                <th key={col} style={{ 
                                    minWidth: "150px",
                                    maxWidth: "200px",
                                    padding: "8px",
                                    textAlign: "center",
                                    whiteSpace: "nowrap",
                                    overflow: "hidden",
                                    textOverflow: "ellipsis",
                                    backgroundColor: "#f8f9fa",
                                    borderBottom: "2px solid #ddd"
                                    
                                }}>
                                    {info[col] || col}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {data.map((row, idx) => (
                            <tr key={row.id + '-' + idx}>
                                {columns.map(col => (
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
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </Table>
            </div>
            <div style={{ margin: '1rem 0', display: 'flex', gap: '1rem', alignItems: 'center', justifyContent: 'flex-end' }}>
                <Button
                    variant="secondary"
                    onClick={() => fetchDate(Math.max(1, page - 1))}
                    disabled={page <= 1 || totalPage <= 1}
                >
                    上一頁
                </Button>
                <span>第 {page} 頁</span>
                
                <Button
                    variant="secondary"
                    onClick={() => fetchDate(Math.min(totalPage, page + 1))}
                    disabled={page >= totalPage || totalPage <= 1}
                >
                    下一頁
                </Button>
                <Form.Control
                    type="number"
                    min={1}
                    max={totalPage}
                    value={page || ""}
                    onChange={e => {
                        const val = Math.max(1, Math.min(totalPage, Number(e.target.value) || 1));
                        setPage(val);
                    }}
                    onBlur={() => {
                        const validPage = Math.max(1, Math.min(totalPage, page));
                        if (validPage !== page) {
                            setPage(validPage);
                        }
                        fetchDate(validPage);
                    }}
                    onKeyDown={e => {
                        if (e.key === 'Enter') {
                            const validPage = Math.max(1, Math.min(totalPage, page));
                            if (validPage !== page) {
                                setPage(validPage);
                            }
                            fetchDate(validPage);
                        }
                    }}
                    style={{ width: 80, display: 'inline-block' }}
                />
                <span>
                    跳頁 | 共 {totalPage} 頁 | 總計 {totalCount} 筆資料
                </span>
            </div>
        </div>
    );
}


export default ChooseReport;