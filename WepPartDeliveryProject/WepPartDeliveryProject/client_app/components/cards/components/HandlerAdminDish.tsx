import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';
import { Button, Col, Row } from 'react-bootstrap';
import Link from 'next/link';

interface HandlerAdminDishProps {
    dishId: string, 
    dishDeleted: boolean, 
    isVisibleForUsers: boolean, 
    changeDeleteStateDish: (dishId:string)=>void,
    changeVisibleStateDish: (dishId:string)=>void,
}

const HandlerAdminDish: FC<HandlerAdminDishProps> = ({ dishId, dishDeleted, isVisibleForUsers, changeDeleteStateDish, changeVisibleStateDish }) => {

    const handleDeleteStateDish = () =>{
        changeDeleteStateDish(dishId);
    }

    const handleVisibleStateDish = () =>{
        changeVisibleStateDish(dishId);
    }

    return (
        <>
            <Row className='ms-1 d-flex justify-content-center'>
                <Col xs='auto' className='mt-1 mt-xxl-0'>
                    <Link href={"/admin/changeDish?dishId=" + dishId} className={`btn btn-secondary`}>
                        Изменить блюдо
                    </Link>
                </Col>
                <Col xs='auto' className='mt-1 mt-xxl-0'>
                    <Button onClick={handleDeleteStateDish} className={`btn btn-secondary`}>
                        {dishDeleted ? "Восстановить блюдо" : "Удалить блюдо"}
                    </Button>
                </Col>
                <Col xs='auto' className='mt-1 mt-xxl-0'>
                    <Button onClick={handleVisibleStateDish} className={`btn btn-secondary`}>
                        {isVisibleForUsers ? "Скрыть от пользователей" : "Сделать доступным для пользователей"}
                    </Button>
                </Col>
            </Row>
        </>
    );
}

export default HandlerAdminDish;

