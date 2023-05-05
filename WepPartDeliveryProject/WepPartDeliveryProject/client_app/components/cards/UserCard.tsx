import React, { FC, useContext, useState } from 'react';
import { Card, Col, Row, Button } from 'react-bootstrap';
import Link from 'next/link';
import { AuthContext } from '../contexts/AuthContext';
import OrderStateItem from './components/OrderStateItem';

const UserCard: FC<orderCardInfo> = (orderInfo) => {
    const roleContextData = useContext<authContextProps>(AuthContext);
    const [showStory, setShowStory] = useState(false);

    const handleCancelClick = (): void => {
        orderInfo.DeleteOrder(orderInfo.id);
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

    return (
        <>
            <Card className="mt-2 bg-light">
                <Row className="g-0 align-items-center">
                    <Col className="col-md">
                        <Card.Body>
                            <Card.Title>Номер заказа: {orderInfo.id}</Card.Title>
                            <Card.Text>Стоимость:{orderInfo.price}</Card.Text>
                            <Card.Text>Адрес доставки: {orderInfo.deliveryAddress}</Card.Text>
                            {orderInfo.story != null && <Card.Text>Статус: {orderInfo.story[orderInfo.story.length-1].nameOfState}</Card.Text>}
                            <Row className='d-flex justify-content-center'>
                                <Col className='col-10 col-lg-auto me-lg-auto mt-2'>
                                    <Link href={(roleContextData.isAdmin ? '/admin':'/profile') + "/order?orderId=" + orderInfo.id} className='btn btn-secondary w-100'>Посмотреть детали</Link>
                                </Col>
                                {roleContextData.isAdmin && <Col className='col-10 col-lg-auto mt-2'>
                                        <Button className='btn btn-danger w-100' onClick={handleCancelClick}>Отменить</Button>
                                    </Col>}
                                <Col className='col-10 col-lg-auto mt-2'>
                                    <Button className='btn btn-secondary w-100' onClick={handleShowStoryClick}>{showStory ? 'Скрыть':'Показать'} историю заказа</Button>
                                </Col>
                                {showStory && orderInfo.story?.map((value,i)=><OrderStateItem key={i} {...value}/>)}
                                {showStory && roleContextData.isAdmin && 
                                    <Row className='d-flex justify-content-center'>
                                        <Col className='col-auto mt-2'>
                                            <Button className='btn btn-danger w-100' onClick={handleMoveToPreviousOrderStage}>Вернуть к предыдущей стадии</Button>
                                        </Col>
                                        <Col className='col-auto mt-2'>
                                            <Button className='btn btn-danger w-100' onClick={handleMoveToNextOrderStage}>Перевести в следующую стадию</Button>
                                        </Col>
                                    </Row>
                                }
                            </Row>
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
        </>
    );
}

export default UserCard;

