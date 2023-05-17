import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { Button, Col, Row } from 'react-bootstrap';

interface CounterOrderDishProps { 
    dishId: string, 
    cancelDish: (dishId: string) => void, 
    changeCountDish: (dishId: string, newCount:number) => void, 
    countOrdered: number,
}

const CounterOrderDish: FC<CounterOrderDishProps> = ({ dishId, cancelDish, changeCountDish, countOrdered }) => {
    const [count, setCount] = useState(countOrdered);

    const handleChangeCountClick = (countToAdd: number) => {
        setCount((count) => {
            let sum = count + countToAdd;
            return sum > 20 || sum < 1 ? count : sum;
        });
    }

    const handleCommitChangeCountClick = async () =>{
        changeCountDish(dishId, count);
    }

    const handleCancelClick = (): void => {
        cancelDish(dishId);
    }

    return (
        <>
            <Row className='d-flex justify-content-end pe-md-3'>
                <Col xs={12} md={4} className='d-flex justify-content-md-start justify-content-center mt-2'>
                    <Button onClick={handleCancelClick} className={`btn btn-danger me-2`}>
                        Отменить
                    </Button>
                </Col>
                <Col xs={12} md={4} className='mt-2'>
                    <Row className='d-flex justify-content-md-end justify-content-center'>
                        <Button onClick={() => handleChangeCountClick(1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                            +
                        </Button>
                        <div className={`d-flex align-items-center justify-content-center ${styles.cardCountBtnAndP}`}>
                            <p className='m-0'>
                                {count}
                            </p>
                        </div>
                        <Button onClick={() => handleChangeCountClick(-1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                            -
                        </Button>
                    </Row>
                </Col>
                <Col xs={12} md={4} className='d-flex justify-content-md-start justify-content-center mt-2'>
                    <Button onClick={handleCommitChangeCountClick}>
                        Изменить количество
                    </Button>
                </Col>
            </Row>
        </>
    );
}

export default CounterOrderDish;

