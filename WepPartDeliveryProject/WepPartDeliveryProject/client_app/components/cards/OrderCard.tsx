import React, { FC, useContext, useState } from 'react';
import { Card, Col, Row } from 'react-bootstrap';
import Link from 'next/link';
import { AuthContext } from '../contexts/AuthContext';
import OrderStateItem from './components/OrderStateItem';
import CancelOrderModal from '../models/CancelOrderModal';
import ReviewOrderModal from '../models/ReviewOrderModal';

const OrderCard: FC<orderCardInfo> = (orderInfo) => {
    const authContextData = useContext<authContextProps>(AuthContext);
    const [showStory, setShowStory] = useState(false);
    const [showCancelModel, setShowCancelModal] = useState(false);
    const [showReviewModel, setShowReviewModal] = useState(false);

    const handleShowCancelModal = () =>{
        setShowCancelModal(true);
    }

    const handleCloseCancelModal = () =>{
        setShowCancelModal(false);
    }

    const handleShowReviewModal = () =>{
        setShowReviewModal(true);
    }

    const handleCloseReviewModal = () =>{
        setShowReviewModal(false);
    }

    const handleCancelClick = (reasonOfCancel:string): void => {
        orderInfo.DeleteOrder(orderInfo.id, reasonOfCancel);
        setShowCancelModal(false);
    }

    const handleSendReviewClick = async (review:string, rating:number)=>{
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/reviewOrder`, {
            method: "POST",
            credentials: "include",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({ orderId: orderInfo.id, review, clientRating:rating })
        });

        if (resp1.ok) {
            setShowReviewModal(false);
        }
        else{
            alert(await resp1.text());
        }
    }

    const handleShowStoryClick = (): void => {
        setShowStory(!showStory);
    }

    const handleMoveToNextOrderStage = ():void => {
        const orderStory = orderInfo.story as orderState[];
        orderInfo.MoveOrderToNextStage(orderInfo.id, orderStory[orderStory.length - 1].orderStateId)
    }

    const handleMoveToPreviousOrderStage = ():void => {
        const orderStory = orderInfo.story as orderState[];
        orderInfo.MoveOrderToPreviousStage(orderInfo.id, orderStory[orderStory.length - 1].orderStateId)
    }

    let clientCanCancelOrder:Boolean = false;
    if(orderInfo.story != null ){
        const curOrderStatus = orderInfo.story[orderInfo.story.length-1];
        clientCanCancelOrder = (authContextData.isClient && curOrderStatus.numberOfStage != 16 && curOrderStatus.numberOfStage != 32) as Boolean;
    }

    let clientCanReviewOrder:Boolean = false;
    if(orderInfo.story != null ){
        const curOrderStatus = orderInfo.story[orderInfo.story.length-1];
        clientCanReviewOrder = (authContextData.isClient && curOrderStatus.numberOfStage == 16  && orderInfo.canWriteReview) as Boolean;
    }

    return (
        <>
            <Card className="mt-2 bg-light">
                <Row className="g-0 align-items-center">
                    <Col className="col-md">
                        <Card.Body>
                            <Card.Title>Номер заказа: {orderInfo.id}</Card.Title>
                            <Row className='mb-2 d-flex justify-content-between'>
                                <Col className='col-auto'>
                                    <Card.Text>Стоимость:{orderInfo.price}</Card.Text>
                                </Col>
                                <Col className='col-auto'>
                                    <Card.Text>Адрес доставки: {orderInfo.deliveryAddress}</Card.Text>
                                </Col>
                                <Col className='col-auto'>
                                    <Card.Text>Номер клиента: {orderInfo.phoneNumber}</Card.Text>
                                </Col>
                            </Row>

                            {orderInfo.story != null && <Card.Text>Статус: {orderInfo.story[orderInfo.story.length-1].nameOfState}</Card.Text>}
                            <Row className='d-flex justify-content-center'>
                                <Col className='col-10 col-lg-auto me-lg-auto mt-2'>
                                    <Link href={(authContextData.isAdmin ? '/admin':'/profile') + "/order?orderId=" + orderInfo.id} className='btn btn-secondary w-100'>Посмотреть детали</Link>
                                </Col>
                                {(authContextData.isAdmin || clientCanCancelOrder) &&
                                    <Col className='col-10 col-lg-auto mt-2'>
                                        <button className='btn btn-danger w-100' onClick={handleShowCancelModal}>Отменить</button>
                                    </Col>}
                                {clientCanReviewOrder &&
                                    <Col className='col-10 col-lg-auto mt-2'>
                                        <button className='btn btn-primary w-100' onClick={handleShowReviewModal}>Написать отзыв</button>
                                    </Col>}
                                <Col className='col-10 col-lg-auto mt-2'>
                                    <button className='btn btn-secondary w-100' onClick={handleShowStoryClick}>{showStory ? 'Скрыть':'Показать'} историю заказа</button>
                                </Col>
                                {showStory && orderInfo.story?.map((value,i)=><OrderStateItem key={i} {...value}/>)}
                                {((showStory && authContextData.isAdmin) || authContextData.isKitchenWorker) && 
                                    <Row className='d-flex justify-content-center'>
                                        <Col className='col-auto mt-2'>
                                            <button className='btn btn-danger w-100' onClick={handleMoveToPreviousOrderStage}>Вернуть к предыдущей стадии</button>
                                        </Col>
                                        <Col className='col-auto mt-2'>
                                            <button className='btn btn-danger w-100' onClick={handleMoveToNextOrderStage}>Перевести в следующую стадию</button>
                                        </Col>
                                    </Row>
                                }
                            </Row>
                            
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
            {showCancelModel && <CancelOrderModal show={showCancelModel} closeModel={handleCloseCancelModal} commitCancelOrder={handleCancelClick}/>}
            {showReviewModel && <ReviewOrderModal show={showReviewModel} closeModel={handleCloseReviewModal} commitAction={handleSendReviewClick}/>}
        </>
    );
}

export default OrderCard;

