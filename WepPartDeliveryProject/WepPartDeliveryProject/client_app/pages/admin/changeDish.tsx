import { ChangeEvent, FC, useState, FormEvent, useEffect } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticProps } from 'next'
import { Row, Col, Form, InputGroup, Button, DropdownButton, Dropdown } from 'react-bootstrap'
import { useRouter } from "next/router"

export const getStaticProps: GetStaticProps = async () => {
    const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categoryList = await resp1.json() as categoryItem[];

    return {
        props: {
            categories: categoryList,
        }
    }
}

const ChangeDish: FC<{ categories: categoryItem[], }> = ({ categories }) => {
    const router = useRouter();
    const dishId = router.query["dishId"];
    const [dish, setDish] = useState<dishAdminInfo>({ id: "", description: "", images: [], isAvailableForUser: true, isDeleted: false, name: "", price: 0, weight: 0 });
    const [selectedCategory, setSelectedCategory] = useState<categoryItem | undefined>();
    const [loadedFilesName, setLoadedFilesName] = useState<string[]>([]);
    const [loadedFiles, setLoadedFiles] = useState<FileList | null>(null);

    useEffect(() => {
        if(dishId == undefined || dishId == null)
            return;
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/getDish/${dishId}`, {
                credentials: 'include',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                }, 
            });
            
            const dishInfo = await resp.json() as {dish: dishAdminInfo, category: categoryItem};
            setDish(dishInfo.dish);
            setSelectedCategory(dishInfo.category);
        }
        fetchData();
    }, [dishId, router]);

    const handleInputData = (e: ChangeEvent<HTMLInputElement>): void => {
        const value:string|boolean = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
        const name:string = e.target.name;
        
        setDish(prevDish => ({ ...prevDish, [name]: value }));
    }

    const handleLoadFiles = (e: ChangeEvent<HTMLInputElement>): void => {
        const value:string[] = [];

        for(let i = 0; i < (e.target.files?.length ?? 0); i++){
            if(e.target.files != null)
                value.push(e.target.files[i].name);
        }

        const name:string = e.target.name;

        setLoadedFiles(e.target.files);
        setLoadedFilesName(value);
    }

    const handleSelectCategory = (eventKey:string|null): void => {
        setSelectedCategory(categories.find(el=>el.id == eventKey));
    }

    const handleSubmitForm = async (e:FormEvent) => {
        e.preventDefault();

        const formData:FormData = new FormData();
        const body:any = {...dish, categoryId: selectedCategory?.id};

        if(loadedFiles != null){
            for(let i = 0; i < loadedFiles.length; i++){
                formData.append('imagesFiles', loadedFiles[i]);
            }
        };

        for(let key in body){
            formData.append(key, body[key]);
        }

        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/changeDish`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            },
            body: formData,
        });

        if(response.ok){
            router.push("/dishes/"+ await response.json());
        }
    }

    return (
        <ClientLayout categories={categories}>
            <h3 className="text-center mb-5 mt-2">Изменение блюда</h3>
            <Form onSubmit={handleSubmitForm} >
                <Row className="mx-2">
                    <Col xs={12} lg={5}>
                        <div className="text-danger">Если вы загрузите изображения, то предыдущие будут удалены</div>
                        <InputGroup className="mb-3">
                            <Form.Control type="file" multiple name="images" onChange={handleLoadFiles}/>
                        </InputGroup>
                        {loadedFilesName.length != 0 && <div>Выбранные файлы: </div>}
                        {loadedFilesName.map((value, i)=> <InputGroup.Text className="mt-1" key={i}>{value}</InputGroup.Text>)}
                    </Col>
                    <Col xs={12} lg={7} className="mt-3 mt-lg-0">
                        <Row>
                            <Col md={12} className="mb-3 flex-grow-1 w-100">
                                <InputGroup >
                                    <InputGroup.Text className="">Название блюда</InputGroup.Text>
                                    <Form.Control type="text" name="name" value={dish.name} onChange={handleInputData} />
                                </InputGroup>
                            </Col>
                            <Col md={4} className="mb-3 pe-0">
                                <InputGroup>
                                    <DropdownButton variant="outline-secondary" title={selectedCategory == undefined ? "Категория" : selectedCategory.name} onSelect={handleSelectCategory}>
                                        {categories.map((value,i) => <Dropdown.Item eventKey={value.id} key={i}>{value.name}</Dropdown.Item>)}
                                    </DropdownButton>
                                </InputGroup>
                            </Col>
                            <Col md={4} className="mb-3 pe-0">
                                <InputGroup>
                                    <Form.Control className="ms-1" type="text" name="weight" value={dish.weight} onChange={handleInputData} />
                                    <InputGroup.Text className="">Грамм</InputGroup.Text>
                                </InputGroup>
                            </Col>
                            <Col md={4} className="mb-3">
                                <InputGroup >
                                    <Form.Control type="text" name="price" value={dish.price} onChange={handleInputData} />
                                    <InputGroup.Text className="">Р</InputGroup.Text>
                                </InputGroup>
                            </Col>
                            <InputGroup className="mb-3">
                                <InputGroup.Text className="">Описание блюда:</InputGroup.Text>
                                <Form.Control as="textarea" type="textarea" name="description" value={dish.description} onChange={handleInputData} />
                            </InputGroup>
                            <Col md={6} className="pe-0">
                                <Form.Switch label="Отметить как удаленное?" name="isDeleted" checked={dish.isDeleted} onChange={handleInputData} />
                            </Col>
                            <Col md={6}>
                                <Form.Switch label="Доступно пользователю?" name="isAvailableForUser" checked={dish.isAvailableForUser} onChange={handleInputData} />
                            </Col>
                        </Row>
                    </Col>
                </Row>
                <Row className="m-2 d-flex justify-content-end">
                    <Col xs={12} md={4}>
                        <Button type="submit" className="w-100">Изменить блюдо</Button>
                    </Col>

                </Row>

            </Form>
        </ClientLayout>
    )
}

export default ChangeDish;