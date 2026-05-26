import React, { useState, useEffect, Suspense } from 'react';
import { Container, Card, Row, Col, Button, Badge, Alert, Form } from 'react-bootstrap';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/GlobalProvider';

const ProductResearching = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const [selectroleOpen, setSelectRoleOpen] = useState(null);

    // 使用正確的相對路徑
    const RollingResearching = React.lazy(() =>
      import("./scrollingResearching/index.js")
    );
    const MixingRecordSearch = React.lazy(() =>
      import("./mixingRecordSearch/index.js")
    );
    const CoatingSearch = React.lazy(() =>
      import("./coatingSearch/index.js")
    );

    // 當選擇變更時，更新URL
    useEffect(() => {
        if (selectroleOpen === 'mixing') {
            navigate('/productResearching/mixingRecordSearch', { replace: true });
        } 
        else if (selectroleOpen === 'rolling') {
            navigate('/productResearching/scrollingResearching', { replace: true });
        }
        else if (selectroleOpen === 'coater') {
            navigate('/productResearching/coatingSearch', { replace: true });
        }
    }, [selectroleOpen, navigate]);

    // 當URL變更時，更新選擇
    useEffect(() => {
        if (location.pathname.includes('/mixingRecordSearch')) {
            setSelectRoleOpen('mixing');
        } else if (location.pathname.includes('/scrollingResearching')) {
            setSelectRoleOpen('rolling');
        } else if (location.pathname.includes('/coatingSearch')) {
            setSelectRoleOpen('coater');
        }
    }, [location]);

    const handleChangePage = (selectroleOpen) => {
        switch (selectroleOpen) {
            case 'mixing':
                return (
                    <Suspense fallback={<div>Loading...</div>}>
                        <MixingRecordSearch />
                    </Suspense>
                );
            case 'rolling':
                return (
                    <Suspense fallback={<div>Loading...</div>}>
                        <RollingResearching />
                    </Suspense>
                );
            case 'coater':
                return (
                    <Suspense fallback={<div>Loading...</div>}>
                        <CoatingSearch />
                    </Suspense>
                );
            default:
                return (
                    null
                );
        }
    };
    
    const roleChangeHidden = () => {
        if (selectroleOpen === 'mixing' || 
            selectroleOpen === 'rolling' ||
            selectroleOpen === 'coater'
        
        ) {
            return { display: "none" };
        }
        return {};
    };


return (
    <Container
        className="d-flex flex-column justify-content-center align-items-center"
        style={{ minHeight: "60vh" }}
    >
        <div style={roleChangeHidden()} className="w-100">
            <div className="text-center mb-4">
                <h2 className="fw-bold text-primary">
                    <i className="bi bi-clipboard-data me-2"></i>
                    生產作業查詢系統
                </h2>
            </div>
            <Form.Group>
                <div className="text-center mb-4">選擇作業類型</div>
                <Form.Select
                    value={selectroleOpen || ""}
                    onChange={(e) => setSelectRoleOpen(e.target.value)}
                >
                    <option value="">請選擇</option>
                    <option value="mixing">正負極混漿</option>
                    <option value="coater">正負極塗佈</option>
                    <option value="rolling">輾壓與分切</option>
                </Form.Select>
            </Form.Group>
        </div>
        {handleChangePage(selectroleOpen)}
    </Container>
);
};

export default ProductResearching;