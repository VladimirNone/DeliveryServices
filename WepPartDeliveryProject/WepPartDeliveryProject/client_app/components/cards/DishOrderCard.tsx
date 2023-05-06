import React, { FC, useContext } from 'react';
import CounterOrderDish from './components/CounterOrderDish';
import { AuthContext } from '../contexts/AuthContext';
import DishStandartLayout from '../structure/DishStandartLayout';

const DishOrderCard: FC<orderedDishClientInfo> = ({count, dishInfo, orderId}) => {
    const roleContextData = useContext<authContextProps>(AuthContext);

    const changeCountOrderedDish = async (dishId:string, newCount: number) =>{
        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/changeCountOrderedDish`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({newCount, orderId, dishId})
        });
    }

    return (
        <DishStandartLayout dishInfo={dishInfo}>
            {roleContextData.isAdmin && <CounterOrderDish dishId={dishInfo.id} cancelDish={dishInfo.DeleteCardFromList} changeCountDish={changeCountOrderedDish} countOrdered={count}/>}
        </DishStandartLayout>
    );
}

export default DishOrderCard;

