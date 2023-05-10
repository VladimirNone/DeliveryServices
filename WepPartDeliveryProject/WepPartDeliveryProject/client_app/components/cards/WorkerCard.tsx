import React, { FC, useState } from 'react';
import { Card, Col, Row, Button } from 'react-bootstrap';

const WorkerCard: FC<{ userInfo: profileInfo }> = ({ userInfo }) => {
    const [showMoreInfo, setShowMoreInfo] = useState(false);

    const handleShowStoryClick = (): void => {
        setShowMoreInfo(!showMoreInfo);
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
                            <Col xs={12} lg='auto' className='mt-1'>
                                <Card.Title>Должность: {userInfo.jobTitle}</Card.Title>
                            </Col>
                        </Row>
                        <Row className='d-flex justify-content-start'>
                            <Col xs={12} lg='auto' className='me-lg-auto mt-1'>
                                <Card.Text>Имя: {userInfo.name}</Card.Text>
                            </Col>
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
                            <Row className='d-flex justify-content-start'>
                                <Col md='auto' className='mt-1'>
                                    <Card.Text>Получил работу: {new Date (userInfo.gotJob as Date).toLocaleDateString()}</Card.Text>
                                </Col>
                                <Col md='auto' className='mt-1'>
                                    <Card.Text>Дата рождения: {new Date(userInfo.born as string).toLocaleDateString()}</Card.Text>
                                </Col>
                                <Col md='auto' className='mt-1'>
                                    <Card.Text>Номер телефона: {userInfo.phoneNumber}</Card.Text>
                                </Col>
                            </Row>
                        }
                    </Card.Body>
                </Row>
            </Card>
        </>
    );
}

export default WorkerCard;

