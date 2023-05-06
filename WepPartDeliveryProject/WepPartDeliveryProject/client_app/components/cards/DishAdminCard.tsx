import React, { FC } from 'react';
import DishStandartLayout from '../structure/DishStandartLayout';
import HandlerAdminDish from './components/HandlerAdminDish';

interface DishAdminCard extends dishAdminInfo {
    changeVisibleState: (dishId:string)=>void, 
    changeDeletedState: (dishId:string)=>void
}

const DishAdminCard: FC<DishAdminCard> = (dishInfo) => {
    return (
        <DishStandartLayout dishInfo={dishInfo}>
            <HandlerAdminDish 
                dishId={dishInfo.id}
                dishDeleted={dishInfo.isDeleted} 
                isVisibleForUsers={dishInfo.isAvailableForUser} 
                changeDeleteStateDish={dishInfo.changeDeletedState}
                changeVisibleStateDish={dishInfo.changeVisibleState}/>
        </DishStandartLayout>
    );
}

export default DishAdminCard;

