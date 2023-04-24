import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';


const CounterMainDish: FC<{dishId: string}> = ({dishId}) => {
    const [count, setCount] = useState(1);
    const [cookies, setCookie] = useCookies(['cartDishes']);    

    //Добавляет количество блюд, к находящихся в корзине
    const addCountDishToCookies = ():void => {
        var countCurDish = cookies.cartDishes[dishId] ?? 0;
        cookies.cartDishes[dishId] = countCurDish + count;

        setCookie('cartDishes', JSON.stringify(cookies.cartDishes), { path: '/', sameSite: "none", secure: true })
    };

    const handleClick = (countToAdd: number): void => {
        setCount((count) => {
            let sum = count + countToAdd;
            return sum > 20 || sum < 1 ? count : sum;
        });
    }

    return (
        <>
            <div className='d-flex justify-content-end pe-md-3'>
                <button onClick={() => handleClick(1)} className={`btn btn-secondary ${styles.cardCountBtnAndP}`}>
                    +
                </button>
                <div className={`d-flex align-items-center justify-content-center ${styles.cardCountBtnAndP}`}>
                    <p className='m-0'>
                        {count}
                    </p>
                </div>
                <button onClick={() => handleClick(-1)} className={`btn btn-secondary me-2 ${styles.cardCountBtnAndP}`}>
                    -
                </button>
                <button className='btn btn-primary' onClick={addCountDishToCookies}>
                    Добавить в корзину
                </button>
            </div>
        </>
    );
}

export default CounterMainDish;

