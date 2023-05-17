import React, { FC, useState } from 'react';
import styles from '@/styles/Home.module.css'
import { useCookies } from 'react-cookie';
import { Button } from 'react-bootstrap';


const CounterMainDish: FC<{dishId: string}> = ({dishId}) => {
    const [count, setCount] = useState(1);
    const [cookies, setCookie] = useCookies(['cartDishes']);    

    //Добавляет количество блюд, к находящимся в корзине
    const addCountDishToCookies = ():void => {
        let futureCookie:any = {};
        if(cookies.cartDishes != undefined)
            futureCookie = cookies.cartDishes;

        var countCurDish = futureCookie[dishId] ?? 0;

        futureCookie[dishId] = countCurDish + count;

        setCookie('cartDishes', JSON.stringify(futureCookie), { path: '/', sameSite: "none", secure: true });
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
                <Button onClick={addCountDishToCookies}>
                    Добавить в корзину
                </Button>
            </div>
        </>
    );
}

export default CounterMainDish;

