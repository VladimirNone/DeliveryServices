import Head from 'next/head'
import { FC, useEffect, useState } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticProps } from 'next'
import DishCartCard from '@/components/cards/DishCartCard'
import { useCookies } from 'react-cookie'
import PlaceAnOrderModel from '@/components/models/PlaceAnOrderModel'
import { stringify } from 'querystring'

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories
        }
    }
}

const Cart: FC<{ categories: categoryItem[] }> = ({ categories }) => {
    const [dishes, setDishes] = useState<dishClientInfo[]>([]);
    const [cookies, setCookie, removeCookie] = useCookies(['cartDishes']);

    const [show, setShow] = useState(false);

    const handleShowModal = () =>{
        setShow(true);
    }

    const handleCloseModal = () =>{
        setShow(false);
    }

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/getCart`, {
                credentials: 'include',
            });
            const cartDishes = await resp.json() as dishClientInfo[];
            setDishes(cartDishes);
        }
        fetchData();
    }, []);

    const handleDeleteItem = (dishId: string): void => {
        setDishes(prevState => prevState.filter(el => el.id != dishId));
    }

    const clearList = (): void => {
        setDishes([]);
        removeCookie('cartDishes');
    }

    const handlePlaceAnOrder = async (userAddress: string, userPhoneNumber: string, comment: string) => {

        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/placeAnOrder`, {
            method: "POST",
            credentials: "include",
            headers: {
                'Content-Type': 'application/json;charset=utf-8',
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            },
            body: JSON.stringify({deliveryAddress: userAddress, phoneNumber: userPhoneNumber, comment})
        });

        if (response.ok) {
            clearList();
            setShow(false);
        }
        else {
            alert(await response.text())
        }
    }

    return (
        <ClientLayout categories={categories}>
            <main className='mb-2'>
                <div>
                    {dishes.map((dish, i) => <DishCartCard key={i} {...dish} DeleteCardFromList={handleDeleteItem} />)}
                </div>
                {dishes.length != 0 ?
                    (<div>
                        <button className='btn btn-primary w-100 mt-2' onClick={handleShowModal}>
                            Оформить заказ
                        </button>
                    </div>)
                    : (
                        <div className='d-flex justify-content-center mt-5'>
                            <h2>
                                Ваша корзина пуста
                            </h2>
                        </div>
                    )
                }
            </main>
            {show && <PlaceAnOrderModel show={show} closeModel={handleCloseModal} commitAction={handlePlaceAnOrder}/>}
        </ClientLayout>
    )
}

export default Cart;