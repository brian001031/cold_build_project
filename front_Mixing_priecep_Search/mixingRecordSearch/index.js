
import  React ,{ useState, useEffect , Suspense } from 'react';
import { Button, Table, Form, InputGroup, DropdownButton, Dropdown } from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import moment from 'moment';
import axios from 'axios';
import 'moment/locale/zh-tw'; 
import config from '../../../config';
import { useNavigate, useLocation } from 'react-router-dom';
import { FormattedMessage, useIntl } from "react-intl";


const ChooseReport = React.lazy(() => import('./chooseReport'));
const DayReport = React.lazy(() => import('./dayReport'));
const IngredientInfo =  React.lazy(() =>import("./ingredientinfo"));


const MixingRecordSearch = () => {

    const [option, setOption] = useState('dayReport');
    const navigate = useNavigate();
    const intl = useIntl();


    const [isSmallScreen, setIsSmallScreen] = useState(window.innerWidth < 1024);

    useEffect(() => {
        const handleResize = () => {
            setIsSmallScreen(window.innerWidth < 1024);
        };
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    const changeTitle = () =>{
        if (isSmallScreen){
            return (
                <>
                    <FormattedMessage id="scrollingResearching.dayReport.titleNickName" defaultMessage="查詢表" />
                </>
            )
        }else {
            return (
                <>
                    <FormattedMessage id="scrollingResearching.dayReport.title" defaultMessage="混漿生產查詢表" />
                </>
            )
        }
    }

    return (
        <div style={{ padding: 24 }}>
            <>
            <Form.Group className="mb-3" style={{ display: "flex", alignItems: "center" }}>
                <div style={{ flex: "0 0 30%", marginRight: "10px" }}>
                    <Form.Label style={{ fontSize: "2rem", fontWeight: "900" }}>
                        {changeTitle()}
                    </Form.Label>
                </div>
                <div style={{ flex: "0 0 33%" }}>
                    <Form.Control as="select" value={option} onChange={(e) => setOption(e.target.value)}>
                        <option value="dayReport">
                            <FormattedMessage id="scrollingResearching.switchBar.ProductionReportNow" defaultMessage="戰報表" />
                        </option>
                        <option value="chooseReport">
                            <FormattedMessage id="scrollingResearching.switchBar.Detailed" defaultMessage="逐筆查詢" />
                        </option>
                        <option value="ingredientinfo">
                            <FormattedMessage id="scrollingResearching.switchBar.RecipeOverview" defaultMessage="配方資訊瀏覽" />
                        </option>                                                
                    </Form.Control>
                </div>
                <div style={{ flex: "1", display: "flex", justifyContent: "flex-end" , alignContent: "center"}}>
                    <Button 
                    variant="outline-secondary" 
                    className="mb-3"
                    style={{ marginLeft: '10px' }}
                    onClick={() => {
                        navigate('/productResearching');
                    }}
                    >
                        {
                            intl.formatMessage({ id: "scrollingResearching.switchBar.ChooseOtherReport", defaultMessage: "選擇其他表單" })
                        }
                    </Button>
                </div>
            </Form.Group>
            </>
            {
                option === "chooseReport" ? (
                    <Suspense fallback={<div>Loading...</div>}>
                        <ChooseReport />
                    </Suspense>
                ) :
                option === "dayReport" ? (
                    <Suspense fallback={<div>Loading...</div>}>
                        <DayReport />
                    </Suspense>
                ) :                        
                option === "ingredientinfo" ? (
                    <Suspense fallback={<div>Loading...</div>}>
                        <IngredientInfo />
                    </Suspense>
                ) :
                null
            }
        </div>
    );
};

export default MixingRecordSearch;