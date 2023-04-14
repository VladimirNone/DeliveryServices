import { FC, useState } from 'react';
import { Card, Col, Row, Carousel, Image } from 'react-bootstrap';
import imageNext from "../../public/суши.png"

const DishClientCard: FC<dishClientCardProps> = (dishInfo) => {
    const [index, setIndex] = useState(0);

    const handleSelect = (selectedIndex: number, e: any) => {
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

                                <button className='btn btn-secondary' style={{ width: '37.6px', height: '37.6px' }}>+</button>
                                <div className='d-flex align-items-center justify-content-center' style={{ width: '37.6px', height: '37.6px' }}>
                                    <p className='m-0'>1</p>
                                </div>
                                <button className='btn btn-secondary me-2' style={{ width: '37.6px', height: '37.6px' }}>-</button>

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

