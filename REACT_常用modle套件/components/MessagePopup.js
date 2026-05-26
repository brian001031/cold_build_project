import React, { useState, useEffect } from 'react';
import { Modal, Button, Alert } from 'react-bootstrap';
import './MessagePopup.scss';

const MessagePopup = ({ 
    show, 
    onHide, 
    type = 'info', // 'success', 'error', 'warning', 'info'
    title,
    message,
    autoClose = false,
    autoCloseDelay = 3000
}) => {
    const [isVisible, setIsVisible] = useState(show);

    useEffect(() => {
        setIsVisible(show);
        
        // 自動關閉功能
        if (show && autoClose) {
            const timer = setTimeout(() => {
                handleClose();
            }, autoCloseDelay);
            
            return () => clearTimeout(timer);
        }
    }, [show, autoClose, autoCloseDelay]);

    const handleClose = () => {
        setIsVisible(false);
        if (onHide) {
            setTimeout(() => onHide(), 300); // 等待動畫完成
        }
    };

    const getIconByType = () => {
        switch (type) {
            case 'success':
                return '✅';
            case 'error':
                return '❌';
            case 'warning':
                return '⚠️';
            default:
                return 'ℹ️';
        }
    };

    const getVariantByType = () => {
        switch (type) {
            case 'success':
                return 'success';
            case 'error':
                return 'danger';
            case 'warning':
                return 'warning';
            default:
                return 'info';
        }
    };

    const getDefaultTitle = () => {
        switch (type) {
            case 'success':
                return '操作成功';
            case 'error':
                return '發生錯誤';
            case 'warning':
                return '注意';
            default:
                return '提示';
        }
    };

    const cancelButton = () =>{
        onHide && onHide();
    }

    return (
        <Modal 
            show={isVisible} 
            onHide={handleClose}
            centered
            backdrop="static"
            keyboard={false}
            className={`message-popup message-popup-${type}`}
        >
            <Modal.Header className={`bg-${getVariantByType()} text-white`}>
                <Modal.Title className="d-flex align-items-center">
                    <span className="me-2" style={{ fontSize: '1.5rem' }}>
                        {getIconByType()}
                    </span>
                    {title || getDefaultTitle()}
                </Modal.Title>
            </Modal.Header>
            
            <Modal.Body className="py-4">
                <Alert variant={getVariantByType()} className="mb-0 border-0 bg-transparent">
                    <div className="text-center">
                        <div className="message-icon mb-3">
                            <span style={{ fontSize: '3rem' }}>
                                {getIconByType()}
                            </span>
                        </div>
                        <div className="message-text" style={{ fontSize: '1.1rem', lineHeight: '1.6' }}>
                            {message}
                        </div>
                    </div>
                </Alert>
            </Modal.Body>
            
            <Modal.Footer className="justify-content-center">
                <Button 
                    variant={getVariantByType()} 
                    onClick={handleClose}
                    size="lg"
                    className="px-4"
                >
                    <i className="fas fa-check me-2"></i>
                    確定
                </Button>
                <Button 
                    variant= "關閉" 
                    onClick={handleClose}
                    size="lg"
                    className="px-4"
                >
                    <i className="fas fa-check me-2"></i>
                    關閉
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default MessagePopup;
