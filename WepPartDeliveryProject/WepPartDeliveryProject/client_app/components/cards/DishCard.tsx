import { FC } from 'react';
import { Card, Col, Row } from 'react-bootstrap';
import Image from 'next/image'
import imageNext from "../../public/суши.png"

const DishCard: FC<{imagePath:string}> = (props:{imagePath:string}) => {
    return (
        <>
            <Card>
                <Row className='g-0'>
                <Col xs={12} sm={5} md={4} lg={3} className='d-flex align-items-center'>
                        <Card.Img src={props.imagePath} alt="."/>
                    </Col>
                    <Col>
                        <Card.Body>
                            <Row className='align-items-center text-nowrap'>
                                <Col xs={9}><h3>Название блюда</h3></Col>
                                <Col ><p className='text-start text-lg-center m-0'>Цена: 350р</p></Col>
                            </Row>
                            <Card.Text>
                                Это более широкая карточка с вспомогательным текстом ниже в качестве естественного перехода к дополнительному контенту
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

export default DishCard;

