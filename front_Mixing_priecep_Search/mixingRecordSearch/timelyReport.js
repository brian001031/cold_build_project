import React,{ useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { Button, Table, Form, InputGroup, DropdownButton, Dropdown } from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import axios from 'axios';
import 'moment/locale/zh-tw'; 
import config from '../../../config';
import { useNavigate, useLocation } from 'react-router-dom';



const TimelyReport = () =>{
    const [data_Collect, setData_Collect] = useState([]);
    const [pageData, setPageData] = useState([]);
    const [nowPage, setNowPage] = useState(1);
    const [loading, setLoading] = useState(false);
    
    // 使用 useRef 來追踪組件是否已卸載
    const isMountedRef = useRef(true);
    
    // 計算最大作業人員數量
    const maxMembers = useMemo(() => {
        if (!data_Collect || Object.keys(data_Collect).length === 0) return 0;
        const allData = Object.values(data_Collect).flat();
        return Math.max(...allData.map(item => item.memberInfo ? item.memberInfo.length : 0));
    }, [data_Collect]);
    
    // 使用 useCallback 來避免不必要的重新創建
    const fetchData = useCallback(async (page = nowPage) => {
        if (loading) return; // 防止重複請求
        
        setLoading(true);
        try{
            const response = await axios.get(
                `${config.apiBaseUrl}/mixprocess/nowReport`,
                // `http://localhost:3009/mixprocess/nowReport`,
                {
                    params: {
                        page: page,
                        pageSize: 20
                    }
                }
            );
            const responseData = response.data;
            console.log("Fetched timely report data:", responseData , typeof responseData);
            
            // 只有當組件還存在時才更新狀態
            if (isMountedRef.current) {
                // console.log("Response data:", response.data.data);
                // console.log("Response pagination:", response.data.pagination);
                
                setData_Collect(responseData.data);
                setPageData(responseData.pagination);
            }
        }catch(error){
            if (isMountedRef.current) {
                console.error("Error fetching data:", error);
            }
        } finally {
            if (isMountedRef.current) {
                setLoading(false);
            }
        }
    }, [nowPage, loading]);



    useEffect(() => {
        fetchData();
        
        const interval = setInterval(() => {
            fetchData();
        }, 60000);
    }, []); 

    const handleDataDisplay = (data_Collect, maxMembers) => {
        // data_Collect is expected to be an object with coaterCathode and coaterAnode

        
        if (!data_Collect || Object.keys(data_Collect).length === 0) {
            return <tr><td colSpan={9 + maxMembers}>無資料</td></tr>;
        }


        const rows = Object.values(data_Collect).flat().map((item, idx) => (
            <tr key={idx}>
                <td style={{ minWidth: '100px' }}>{item.tableType || '-'}</td>
                <td style={{ minWidth: '150px' }}>{item.LotCount || '-'}</td>
                <td style={{ minWidth: '100px' }}>{item.Nvalue || '-'}</td>
                <td style={{ minWidth: '120px' }}>{!isNaN(parseFloat(item.Viscosity)) ? parseFloat(item.Viscosity).toFixed(2) : '-'}</td>
                <td style={{ minWidth: '120px' }}>{!isNaN(parseFloat(item.ParticalSize)) ? parseFloat(item.ParticalSize).toFixed(2) : '-'}</td>
                <td style={{ minWidth: '120px' }}>{!isNaN(parseFloat(item.SolidContent)) ? parseFloat(item.SolidContent).toFixed(2) : '-'}</td>
                {Array.from({length: maxMembers}, (_, i) => <td key={i} style={{ minWidth: '100px' }}>{item.memberInfo && item.memberInfo[i] ? item.memberInfo[i] : '-'}</td>)}
            </tr>
        ));

        return rows;
    }



    return (
        <div style={{ backgroundColor: '#ad1e61ff', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)' ,overflowX: 'scroll' }}>
            <div style = {{ fontSize: '24px', fontWeight: 'bold', marginBottom: '5px' , color: 'white' }}>
                即時戰報 {loading && <span style={{ fontSize: '16px' }}>載入中...</span>}
            </div>
            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th style={{ minWidth: '100px' }}>站別</th>
                        <th style={{ minWidth: '150px' }}>混漿批數</th>
                        <th style={{ minWidth: '100px' }}>N值(最後一筆)</th>
                        <th style={{ minWidth: '100px' }}>黏度(最後一筆)</th>
                        <th style={{ minWidth: '100px' }}>粒徑(最後一筆)</th>
                        <th style={{ minWidth: '100px' }}>固含量(最後一筆)</th>
                    </tr>
                </thead>
                <tbody>
                    {Object.keys(data_Collect).length === 0 ? (
                        <tr>
                            <td colSpan={9 + maxMembers}>{loading ? '載入中...' : '無資料'}</td>
                        </tr>
                    ) : (
                        handleDataDisplay(data_Collect, maxMembers)
                    )}
                </tbody>
            </Table>
           
        </div>
    );

}

export default TimelyReport;