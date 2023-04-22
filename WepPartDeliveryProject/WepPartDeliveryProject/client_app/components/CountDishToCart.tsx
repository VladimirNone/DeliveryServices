import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';

type dishCookieData = {
    id: string,
    count: number,
}

const CountDishToCart: FC<{dishId: string}> = ({dishId}) => {
    const [count, setCount] = useState(1);
    const [cookies, setCookie] = useCookies(['cartDishes']);

    const addDishToCookies = ():void => {
        const dishToAdd: dishCookieData = {id:dishId, count: count};

        if(cookies.cartDishes === undefined || cookies.cartDishes === null){
            setCookie('cartDishes', JSON.stringify([dishToAdd]), { path: '/' })
        }
        else{
            const addedDishes: dishCookieData[] = cookies.cartDishes;
            addedDishes.push(dishToAdd);

            setCookie('cartDishes', JSON.stringify(addedDishes), { path: '/' })
        }
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
                <button className='btn btn-primary' onClick={addDishToCookies}>
                    Добавить в корзину
                </button>
            </div>
        </>
    );
}

export default CountDishToCart;

