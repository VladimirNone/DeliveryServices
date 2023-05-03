import React, { FC, useContext, useState } from 'react';
import { Card, Col, Row, Carousel, Image } from 'react-bootstrap';
import styles from '@/styles/Home.module.css'
import Link from 'next/link';
import CounterOrderDish from './components/CounterOrderDish';
import { RoleContext } from '../contexts/RoleContext';

const DishOrderCard: FC<orderedDishClientInfo> = ({count, dishInfo, orderId}) => {
    const [index, setIndex] = useState(0);
    const roleContextData = useContext<roleContextProps>(RoleContext);

    const handleSelect = (selectedIndex: number):void => {
        setIndex(selectedIndex);
    };

    const changeCountOrderedDish = async (dishId:string, newCount: number) =>{
        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/changeCountOrderedDish`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({newCount, orderId, dishId})
        });
    }

    return (
        <>
            <Card>
                <Row className='g-0'>
                    <Col xs={12} sm={5} md={4} lg={3}>
                        <Carousel activeIndex={index} onSelect={handleSelect}>
                            {dishInfo.images?.slice(0,3).map((value, i)=>
                                <Carousel.Item key={i}>
                                    <Image className="d-block w-100" src={value} alt="First slide"/>
                                </Carousel.Item>
                            )}
                        </Carousel>
                    </Col>
                    <Col xs={12} sm={7} md={8} lg={9}>
                        <Card.Body>
                            <Row className='align-items-center'>
                                <Col xs={9}>
                                    <Link href={'/dishes/' + dishInfo.id} className={`${styles.linkWithoutDefaultStyles}`}>
                                        <h3>{dishInfo.name}</h3>
                                    </Link>
                                </Col>
                                <Col>
                                    <Row >
                                        <Col xs={12} >
                                            <p className='text-start m-0'>Цена: {dishInfo.price}р</p>
                                        </Col>
                                        <Col xs={12} >
                                            <p className='text-start m-0'>Стоимость: {dishInfo.price*count}р</p>
                                        </Col>
                                    </Row>
                                </Col>
                            </Row>
                            <Card.Text>
                                {dishInfo.description}
                            </Card.Text>
                            {roleContextData.isAdmin && <CounterOrderDish dishId={dishInfo.id} cancelDish={dishInfo.DeleteCardFromList} changeCountDish={changeCountOrderedDish} countOrdered={count}/>}
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
        </>
    );
}

export default DishOrderCard;

