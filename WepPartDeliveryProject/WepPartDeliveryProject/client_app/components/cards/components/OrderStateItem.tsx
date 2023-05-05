import React, { FC, useState } from 'react';
import { Card, Col, Row } from 'react-bootstrap';

const OrderStateItem: FC<orderState> = (orderState) => {

    return (
        <Card className='mt-2'>
            <Row className='p-md-1'>
                <Col xs={12} md={5} >
                    <div>Статус: {orderState.nameOfState}</div>
                </Col>
                <Col xs={12} md={7} >
                    <div>Время начала стадии: {orderState.timeStartState.toLocaleString()}</div>
                </Col>
            </Row>
            <Row className='p-md-1'>
                <Col xs={12} >
                    <div>Описание стадии: {orderState.descriptionForClient}</div>
                </Col>
            </Row>
            <Row className='p-md-1'>
                <Col xs={12} >
                    <div>Комментарий: {orderState.comment}</div>
                </Col>
            </Row>
        </Card>
    );
}

export default OrderStateItem;

