import React, { FC, useState } from 'react';
import { Card, Col, Row, Carousel, Image } from 'react-bootstrap';
import styles from '@/styles/Home.module.css'
import Link from 'next/link';
import CounterCartDish from './components/CounterCartDish';

const DishCartCard: FC<dishClientInfo> = (dishInfo) => {
    const [index, setIndex] = useState(0);

    const handleSelect = (selectedIndex: number):void => {
        setIndex(selectedIndex);
    };

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
                                <Col ><p className='text-start text-lg-center m-0'>Цена: {dishInfo.price}р</p></Col>
                            </Row>
                            <Card.Text>
                                {dishInfo.description}
                            </Card.Text>
                            <CounterCartDish dishId={dishInfo.id} cancelDish={dishInfo.DeleteCartFromList}/>
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
        </>
    );
}

export default DishCartCard;

