import React, { FC, useState } from 'react';
import { Card, Col, Row, Carousel, Image } from 'react-bootstrap';
import imageNext from "../../public/суши.png"
import styles from '@/styles/Home.module.css'

const DishClientCard: FC<dishClientCardProps> = (dishInfo) => {
    const [index, setIndex] = useState(0);
    const [count, setCount] = useState(1);

    const handleClick = (countToAdd: number) =>{
        setCount((count) => {
            let sum = count + countToAdd;
            return sum > 20 || sum < 0 ? count : sum;
        });
    }

    const handleSelect = (selectedIndex: number) => {
        setIndex(selectedIndex);
    };

    return (
        <>
            <Card>
                <Row className='g-0'>
                    <Col xs={12} sm={5} md={4} lg={3} className='d-flex align-items-center'>
                        <Carousel activeIndex={index} onSelect={handleSelect}>
                            {dishInfo.images.slice(0,2).map((value, i)=>
                                <Carousel.Item key={i}>
                                    <Image className="d-block w-100" src={value} alt="First slide"/>
                                </Carousel.Item>
                            )}
                        </Carousel>
                    </Col>
                    <Col>
                        <Card.Body>
                            <Row className='align-items-center text-nowrap'>
                                <Col xs={9}><h3>{dishInfo.name}</h3></Col>
                                <Col ><p className='text-start text-lg-center m-0'>Цена: {dishInfo.price}р</p></Col>
                            </Row>
                            <Card.Text>
                                {dishInfo.description}
                            </Card.Text>
                            <div className='d-flex justify-content-end pe-md-3'>

                                <button onClick={()=>handleClick(1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>+</button>
                                <div className={`d-flex align-items-center justify-content-center ${styles.cardCountBtnAndP}`}>
                                    <p className='m-0'>{count}</p>
                                </div>
                                <button onClick={()=>handleClick(-1)} className={`btn btn-secondary me-2 ${styles.cardCountBtnAndP}`}>-</button>

                                <button className='btn btn-primary'>Добавить в корзину</button>
                            </div>
                        </Card.Body>
                    </Col>
                </Row>
            </Card>
        </>
    );
}

export default DishClientCard;

