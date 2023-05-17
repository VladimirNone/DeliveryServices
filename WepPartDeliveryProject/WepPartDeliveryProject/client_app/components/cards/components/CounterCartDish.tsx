import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';
import { Button, Col, Row } from 'react-bootstrap';


const CounterCartDish: FC<{ dishId: string, cancelDish: (dishId: string) => void }> = ({ dishId, cancelDish }) => {
    const [cookies, setCookie] = useCookies(['cartDishes']);
    const [count, setCount] = useState(Number.parseInt(cookies?.cartDishes[dishId]) ?? 1);

    //Изменяет количество блюд, находящихся в корзине
    const changeCountDishToCookies = (): void => {
        cookies.cartDishes[dishId] = count;

        setCookie('cartDishes', JSON.stringify(cookies.cartDishes), { path: '/', sameSite: "none", secure: true })
    };

    const handleChangeCountClick = (countToAdd: number): void => {
        setCount((count) => {
            let sum = count + countToAdd;
            return sum > 20 || sum < 1 ? count : sum;
        });
    }

    //Удаляет свойство с id текущего блюда из куки, обновляет куки, удаляет блюдо из списка
    const handleCancelClick = (): void => {
        delete cookies.cartDishes[dishId];
        setCookie('cartDishes', JSON.stringify(cookies.cartDishes), { path: '/', sameSite: "none", secure: true })
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
                    <Button onClick={changeCountDishToCookies}>
                        Изменить количество
                    </Button>
                </Col>
            </Row>
        </>
    );
}

export default CounterCartDish;

