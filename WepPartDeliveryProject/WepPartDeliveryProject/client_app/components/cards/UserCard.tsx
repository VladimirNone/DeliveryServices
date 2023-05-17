import React, { FC, ReactElement, useEffect, useState } from 'react';
import { Card, Col, Row, Button, Form } from 'react-bootstrap';

const UserCard: FC<{ userInfo: profileInfo, markUser: (userId: string) => void, unmarkUser: (userId: string) => void, }> = ({ userInfo, markUser, unmarkUser }) => {
    const [showMoreInfo, setShowMoreInfo] = useState(false);
    const [unmarked, setUnmarked] = useState(true);

    const handleShowStoryClick = (): void => {
        setShowMoreInfo(!showMoreInfo);
    }

    const handleClickToCheckBox = (): void => {
        if (unmarked) {
            markUser(userInfo.id as string);
        }
        else {
            unmarkUser(userInfo.id as string);
        }
        setUnmarked(!unmarked);
    }

    return (
        <>
            <Card className="mt-2 bg-light">
                <Row className="g-0 align-items-center">
                    <Card.Body>
                        <Row className='d-flex justify-content-start'>
                            <Col xs={12} lg='auto' className='mt-1'>
                                <Card.Title>Id: {userInfo.id}</Card.Title>
                            </Col>
                            <Col xs={12} lg='auto' className='mt-1 me-auto'>
                                <Card.Text className='m-0'>Роли пользователя: {userInfo.roles}</Card.Text>
                            </Col>
                            <Col xs={12} lg='auto' className='mt-1 d-flex justify-content-end me-auto '>
                                <Form className='d-flex justify-content-end'>
                                    <Form.Check reverse type="switch" onClick={handleClickToCheckBox} label="Выбрать пользователя" />
                                </Form>
                            </Col>
                        </Row>
                        <Row className='d-flex justify-content-start'>
                            <Col xs={12} lg='auto' className='me-lg-auto mt-1'>
                                <Card.Text>Логин пользователя: {userInfo.login}</Card.Text>
                            </Col>
                            {userInfo.isBlocked &&
                                <Col xs={12} lg='auto' className='mt-1'>
                                    <Card.Text className='text-danger'>Заблокирован</Card.Text>
                                </Col>}
                            <Col xs={12} lg='auto' className='mt-1'>
                                <Button className='btn btn-secondary w-100' onClick={handleShowStoryClick}>{showMoreInfo ? 'Скрыть' : 'Показать'} подробную информацию</Button>
                            </Col>
                        </Row>
                        {showMoreInfo &&
                            <>
                                <Row className='d-flex justify-content-start'>
                                    <Col md='auto' className='mt-1'>
                                        <Card.Text>Имя: {userInfo.name}</Card.Text>
                                    </Col>
                                    <Col md='auto' className='mt-1'>
                                        <Card.Text>Дата рождения: {new Date(userInfo.born as string).toLocaleDateString()}</Card.Text>
                                    </Col>
                                    <Col md='auto' className='mt-1'>
                                        <Card.Text>Номер телефона: {userInfo.phoneNumber}</Card.Text>
                                    </Col>
                                </Row>
                                <Row className='d-flex justify-content-start'>
                                    <Col md='auto' className='mt-1'>
                                        <Card.Text>Адрес: {userInfo.address}</Card.Text>
                                    </Col>
                                </Row>
                            </>
                        }
                    </Card.Body>
                </Row>
            </Card>
        </>
    );
}

export default UserCard;

