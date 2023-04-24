import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';


const CounterCartDish: FC<{dishId: string, cancelDish: (dishId:string)=>void}> = ({dishId, cancelDish}) => {
    const [cookies, setCookie] = useCookies(['cartDishes']);
    const [count, setCount] = useState(Number.parseInt(cookies.cartDishes[dishId]) ?? 1);

    //Изменяет количество блюд, находящихся в корзине
    const changeCountDishToCookies = ():void => {
        cookies.cartDishes[dishId] = count;

        setCookie('cartDishes', JSON.stringify(cookies.cartDishes), { path: '/', sameSite: "none", secure: true })
    };

    const handleChangeCountClick = (countToAdd: number): void => {
        setCount((count) => {
            let sum = count + countToAdd;
            return sum > 20 || sum < 1 ? count : sum;
        });
    }

    const handleCancelClick = ():void =>{
        delete cookies.cartDishes[dishId];
        setCookie('cartDishes', JSON.stringify(cookies.cartDishes), { path: '/', sameSite: "none", secure: true })
        cancelDish(dishId);
    }

    return (
        <>
            <div className='d-flex justify-content-end pe-md-3'>
                <button onClick={handleCancelClick} className={`btn btn-danger me-2`}>
                    Отменить
                </button>
                <button onClick={() => handleChangeCountClick(1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                    +
                </button>
                <div className={`d-flex align-items-center justify-content-center ${styles.cardCountBtnAndP}`}>
                    <p className='m-0'>
                        {count}
                    </p>
                </div>
                <button onClick={() => handleChangeCountClick(-1)} className={`btn btn-secondary me-2 ${styles.cardCountBtnAndP}`}>
                    -
                </button>
                <button className='btn btn-primary' onClick={changeCountDishToCookies}>
                    Изменить количество
                </button>
            </div>
        </>
    );
}

export default CounterCartDish;

