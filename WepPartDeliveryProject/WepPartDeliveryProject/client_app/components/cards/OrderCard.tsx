import React, { FC, useContext } from 'react';
import { Card, Col, Row, Button } from 'react-bootstrap';
import Link from 'next/link';
import { RoleContext } from '../contexts/RoleContext';

const OrderCard: FC<orderCardInfo> = (orderInfo) => {
    const roleContextData = useContext<roleContextProps>(RoleContext);

    const handleCancelClick = (): void => {
        orderInfo.DeleteCardFromList(orderInfo.id);
    }

    return (
        <>
            <Card className="mt-3 bg-light">
                <Row className="g-0 align-items-center">
                    <Col className="col-md">
                        <Card.Body>
                            <h5 className="card-title">Номер заказа: {orderInfo.id}</h5>
                            <p className="card-text">Стоимость:{orderInfo.price}</p>
                            <p className="card-text">Адрес доставки: {orderInfo.deliveryAddress}</p>
                            <p className="card-text">Статус: </p>
                            <Row className='d-flex justify-content-center'>
                                <Col className='col-10 col-lg-auto me-lg-auto mt-2'>
                                    <Link href={"/profile/order?orderId=" + orderInfo.id} className='btn btn-secondary w-100'>Посмотреть детали</Link>
                                </Col>
                                {roleContextData.isAdmin && <Col className='col-10 col-lg-auto mt-2'>
                                        <Button className='btn btn-danger w-100' onClick={handleCancelClick}>Отменить</Button>
                                    </Col>}
                                <Col className='col-10 col-lg-auto mt-2'>
                                    <Button className='btn btn-secondary  w-100'>Показать историю заказа</Button>
                                </Col>
                            </Row>
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
        </>
    );
}

export default OrderCard;

